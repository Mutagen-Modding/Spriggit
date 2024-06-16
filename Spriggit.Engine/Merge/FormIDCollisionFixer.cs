using System.IO.Abstractions;
using System.Reflection;
using LibGit2Sharp;
using Loqui;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;
using Noggog.IO;
using Serilog;
using Spriggit.Core;
using Spriggit.Engine.Services.Singletons;

namespace Spriggit.Engine.Merge;

public class FormIDCollisionFixer
{
    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystem;
    private readonly FormIDReassigner _reassigner;
    private readonly IEntryPointCache _entryPointCache;
    private readonly GetMetaToUse _getMetaToUse;
    private readonly GitFolderLocator _gitFolderLocator;
    private readonly FormIDCollisionDetector _detector;

    public FormIDCollisionFixer(
        ILogger logger,
        IFileSystem fileSystem,
        FormIDReassigner reassigner,
        IEntryPointCache entryPointCache,
        GetMetaToUse getMetaToUse,
        GitFolderLocator gitFolderLocator,
        FormIDCollisionDetector detector)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _reassigner = reassigner;
        _entryPointCache = entryPointCache;
        _getMetaToUse = getMetaToUse;
        _gitFolderLocator = gitFolderLocator;
        _detector = detector;
    }

    public async Task DetectAndFix(
        DirectoryPath spriggitModPath)
    {
        var meta = await _getMetaToUse.Get(null, spriggitModPath, CancellationToken.None);
        var entryPoint = await _entryPointCache.GetFor(meta.ToMeta(), CancellationToken.None);
        if (entryPoint == null)
        {
            throw new NullReferenceException($"Could not construct entry point for {meta}");
        }
        
        var typeStr = $"Mutagen.Bethesda.{meta.Release.ToCategory()}.{meta.Release.ToCategory()}Mod";
        var regis = LoquiRegistration.GetRegisterByFullName(typeStr);
        if (regis == null)
        {
            throw new Exception($"No loqui registration found for {typeStr}");
        }

        var method = this.GetType().GetMethod("DetectAndFixInternal", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var genMethod = method.MakeGenericMethod(new Type[] { regis.SetterType, regis.GetterType });
        genMethod.Invoke(this, new object?[]
        {
            entryPoint,
            spriggitModPath,
            meta
        });
    }

    internal async Task DetectAndFixInternal<TMod, TModGetter>(
        IEntryPoint entryPoint,
        DirectoryPath spriggitModPath,
        SpriggitEmbeddedMeta meta)
        where TMod : class, IContextMod<TMod, TModGetter>, TModGetter
        where TModGetter : class, IContextGetterMod<TMod, TModGetter>
    {
        using var tmp = TempFolder.Factory(fileSystem: _fileSystem);

        FilePath origMergedModPath = Path.Combine(tmp.Dir, "MergedOrig", meta.ModKey.FileName);
        origMergedModPath.Directory?.Create(_fileSystem);

        _logger.Information("Deserializing mod to {Path}", origMergedModPath);
        await entryPoint.Deserialize(
            spriggitModPath,
            origMergedModPath,
            workDropoff: null,
            fileSystem: _fileSystem,
            streamCreator: null,
            cancel: CancellationToken.None);

        var origMergedMod = ModInstantiator<TMod>.Importer(origMergedModPath.Path, meta.Release, fileSystem: _fileSystem);
        
        _logger.Information("Locating collisions");
        var collisions = _detector.LocateCollisions(origMergedMod);
        if (collisions.Count == 0)
        {
            _logger.Information("No collisions found. Exiting");
            return;
        }

        foreach (var coll in collisions)
        {
            if (coll.Value.Count != 2)
            {
                throw new Exception($"Collision detected with not exactly two participants: {string.Join(", ", coll.Value.Take(50))}");
            }
        }

        var toReassign = collisions.SelectMany(x => x.Value)
            .Select(x => x.ToFormLinkInformation())
            .ToArray();

        _logger.Information("Reassigning:");
        foreach (var id in toReassign)
        {
            _logger.Information($"  {id}");
        }

        var gitRootPath = _gitFolderLocator.Get(spriggitModPath);
        
        _logger.Information("Creating repo at {Path}", gitRootPath);

        var repo = new LibGit2Sharp.Repository(gitRootPath);
        if (repo == null)
        {
            throw new Exception("No git repository detected");
        }

        if (!repo.Head.Commits.Any())
        {
            throw new Exception("Git repository had no commits at HEAD");
        }

        var fixSignature = repo.Head.Tip.Author;
        var commit = repo.Head.Commits.First();
        var parents = commit.Parents
            .ToArray();
        
        _logger.Information("Considering commit {Sha}", commit.Sha);

        if (parents.Length != 2)
        {
            throw new Exception("Git did not have a merge commit at HEAD");
        }
        
        _logger.Information("Parent commits:");
        _logger.Information($"  {parents[0].Sha}");
        _logger.Information($"  {parents[1].Sha}");
        
        var origBranch = repo.Head;
        var branchName = $"Spriggit-Merge-Fix-{origBranch.Tip.Sha.Substring(0, 6)}";
        
        _logger.Information("Creating branch {Branch} at {Sha}", branchName, parents[0].Sha);
        
        repo.Branches.Remove(branchName);
        var branch = repo.CreateBranch(branchName, parents[0]);
        Commands.Checkout(repo, branch);

        _logger.Information("Deserializing mod to {Path}", origMergedModPath);        
        await entryPoint.Deserialize(
            spriggitModPath,
            origMergedModPath,
            workDropoff: null,
            fileSystem: _fileSystem,
            streamCreator: null,
            cancel: CancellationToken.None);

        var branchMod = ModInstantiator<TMod>.Importer(origMergedModPath.Path, meta.Release, fileSystem: _fileSystem);

        _logger.Information("Executing reassignment");
        _reassigner.Reassign<TMod, TModGetter>(
            branchMod, 
            () => origMergedMod.GetNextFormKey(),
            toReassign);
        
        _logger.Information("Writing reassigned mod to {Path}", origMergedModPath.Path);
        branchMod.WriteToBinary(origMergedModPath.Path);
        
        _logger.Information("Serializing mod to {Path}", spriggitModPath);  
        await entryPoint.Serialize(
            origMergedModPath.Path,
            spriggitModPath,
            meta.Release,
            workDropoff: null,
            fileSystem: _fileSystem,
            streamCreator: null,
            meta: meta.Source,
            cancel: CancellationToken.None);

        _logger.Information("Committing changes");
        Commands.Checkout(repo, branch);
        Commands.Stage(repo, Path.Combine(spriggitModPath, "*"));
        repo.Commit("FormID Collision Fix", fixSignature, fixSignature, new CommitOptions());

        _logger.Information("Checking out original branch {Branch}", origBranch.CanonicalName);
        Commands.Checkout(repo, origBranch);
        _logger.Information("Merging fix into {Branch}", origBranch.CanonicalName);
        repo.Merge(branch, fixSignature);
        
        _logger.Information("Deleting fix branch {Branch}", branch.CanonicalName);
        repo.Branches.Remove(branch);
        
        FilePath newMergedModPath = Path.Combine(tmp.Dir, "MergedNew", meta.ModKey.FileName);
        _logger.Information("Creating result temp folder {Path}", newMergedModPath.Directory);
        newMergedModPath.Directory?.Create(_fileSystem);

        _logger.Information("Deserializing mod to {Path}", newMergedModPath);  
        await entryPoint.Deserialize(
            spriggitModPath,
            newMergedModPath,
            workDropoff: null,
            fileSystem: _fileSystem,
            streamCreator: null,
            cancel: CancellationToken.None);

        _logger.Information("Checking for collisions in fixed result");
        var newMergedMod = ModInstantiator<TMod>.Importer(newMergedModPath.Path, meta.Release, fileSystem: _fileSystem);

        var newCollisions = _detector.LocateCollisions(newMergedMod);
        if (newCollisions.Count != 0)
        {
            throw new Exception($"Fix still had collided FormIDs.  Leaving in a bad state");
        }
        
        _logger.Information("Collision fix complete");
    }
}