using System.IO.Abstractions;
using LibGit2Sharp;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;
using Noggog.IO;
using Spriggit.Core;

namespace Spriggit.Engine.Collision;

public class FormIDCollisionFixer
{
    private readonly IFileSystem _fileSystem;
    private readonly FormIDReassigner _reassigner;
    private readonly FormIDCollisionDetector _detector;

    public FormIDCollisionFixer(
        IFileSystem fileSystem,
        FormIDReassigner reassigner,
        FormIDCollisionDetector detector)
    {
        _fileSystem = fileSystem;
        _reassigner = reassigner;
        _detector = detector;
    }

    public async Task DetectAndFix<TMod, TModGetter>(
        ModKey modKey,
        GameRelease release,
        IEntryPoint entryPoint,
        DirectoryPath gitRootPath,
        DirectoryPath spriggitModPath)
        where TMod : class, IContextMod<TMod, TModGetter>, TModGetter
        where TModGetter : class, IContextGetterMod<TMod, TModGetter>
    {
        using var tmp = TempFolder.Factory(fileSystem: _fileSystem);

        var mergedModPath = Path.Combine(tmp.Dir, modKey.FileName);

        await entryPoint.Deserialize(
            spriggitModPath,
            mergedModPath,
            workDropoff: null,
            fileSystem: _fileSystem,
            streamCreator: null,
            cancel: CancellationToken.None);

        var mergedMod = ModInstantiator<TMod>.Importer(mergedModPath, release, fileSystem: _fileSystem);
        
        var collisions = _detector.LocateCollisions(mergedMod);
        if (collisions.Count == 0) return;

        foreach (var coll in collisions)
        {
            if (coll.Value.Count != 2)
            {
                throw new Exception($"Collision detected with not exactly two participants: {string.Join(", ", coll.Value)}");
            }
        }

        var toReassign = collisions.SelectMany(x => x.Value.Skip(1))
            .Select(x => x.ToFormLinkInformation())
            .ToArray();

        var repo = new LibGit2Sharp.Repository(gitRootPath);
        if (repo == null)
        {
            throw new Exception("No git repository detected");
        }

        if (!repo.Head.Commits.Any())
        {
            throw new Exception("Git repository had no commits at HEAD");
        }
        
        var parents = repo.Head.Commits.First().Parents
            .ToArray();

        if (parents.Length != 2)
        {
            throw new Exception("Git did not have a merge commit at HEAD");
        }

        var branch = repo.CreateBranch("Spriggit-Merge-Fix", parents[0]);
        Commands.Checkout(repo, branch);

        var branchMod = ModInstantiator<TMod>.Importer(mergedModPath, release, fileSystem: _fileSystem);

        _reassigner.Reassign<TMod, TModGetter>(
            branchMod, 
            () => mergedMod.GetNextFormKey(),
            toReassign);

        //
        // var cache = mergedMod.ToMutableLinkCache<TMod, TModGetter>();
        //
        // var remapping = new Dictionary<FormKey, FormKey>();
        //
        // foreach (var collList in collisions)
        // {
        //     if (collList.Value.Count == 0) continue;
        //
        //     foreach (var other in collList.Value.Skip(1))
        //     {
        //         var context = cache.ResolveContext(other.FormKey, other.GetType());
        //         mod.Remove(other.FormKey, other.GetType());
        //         var newRec = context.DuplicateIntoAsNewRecord(mod);
        //         remapping[]
        //     }
        // }
    }
}