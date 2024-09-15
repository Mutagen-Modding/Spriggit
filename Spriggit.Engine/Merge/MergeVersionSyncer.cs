using LibGit2Sharp;
using Noggog;
using Noggog.IO;
using NuGet.Versioning;
using Spriggit.Core;
using Spriggit.Engine.Services.Singletons;

namespace Spriggit.Engine.Merge;

public class MergeVersionSyncer
{
    private readonly IEntryPointCache _entryPointCache;
    private readonly SpriggitExternalMetaPersister _externalMetaPersister;
    private readonly GetMetaToUse _getMetaToUse;
    private readonly GitFolderLocator _gitFolderLocator;
    
    public MergeVersionSyncer(
        GetMetaToUse getMetaToUse,
        IEntryPointCache entryPointCache,
        SpriggitExternalMetaPersister externalMetaPersister,
        GitFolderLocator gitFolderLocator)
    {
        _getMetaToUse = getMetaToUse;
        _entryPointCache = entryPointCache;
        _externalMetaPersister = externalMetaPersister;
        _gitFolderLocator = gitFolderLocator;
    }

    public async Task DetectAndFix(
        DirectoryPath spriggitModPath,
        DirectoryPath? dataFolder)
    {
        var gitRootPath = _gitFolderLocator.Get(spriggitModPath);
        
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
        
        var parents = repo.Head.Commits.First().Parents
            .ToArray();

        if (parents.Length != 2)
        {
            throw new Exception("Git did not have a merge commit at HEAD");
        }

        var lhsSha = parents[0].Sha;
        var rhsSha = parents[1].Sha;

        var lhsMeta = GetEmbeddedMeta(repo, lhsSha, spriggitModPath);
        var rhsMeta = GetEmbeddedMeta(repo, rhsSha, spriggitModPath);
        if (lhsMeta.Release != rhsMeta.Release)
        {
            throw new ArgumentException($"Releases did not match {lhsMeta.Release} != {rhsMeta.Release}");
        }

        if (lhsMeta.Source.PackageName != rhsMeta.Source.PackageName)
        {
            throw new ArgumentException($"PackageName did not match {lhsMeta.Source.PackageName} != {rhsMeta.Source.PackageName}");
        }

        if (lhsMeta.Source.Version == rhsMeta.Source.Version)
        {
            // Nothing to fix
            return;
        }

        if (!NuGetVersion.TryParse(lhsMeta.Source.Version, out var lhsVersion))
        {
            throw new ArgumentException($"Could not convert to nuget version {lhsMeta.Source.Version}");
        }
        if (!NuGetVersion.TryParse(rhsMeta.Source.Version, out var rhsVersion))
        {
            throw new ArgumentException($"Could not convert to nuget version {rhsMeta.Source.Version}");
        }

        var minVersion = new NuGetVersion(0, 20, 0);
        if (lhsVersion < minVersion || rhsVersion < minVersion)
        {
            throw new ArgumentException($"Minimum spriggit version compatible with this feature is {minVersion}");
        }

        string oldSha, newSha;
        if (lhsVersion < rhsVersion)
        {
            oldSha = lhsSha;
            newSha = rhsSha;
        }
        else
        {
            oldSha = rhsSha;
            newSha = lhsSha;
        }
        
        await ExecuteUpgrade(
            repo: repo, 
            oldSha: oldSha,
            newSha: newSha,
            spriggitPath: spriggitModPath,
            dataFolder: dataFolder);
    }

    private SpriggitModKeyMeta GetEmbeddedMeta(
        Repository repo,
        string sha,
        DirectoryPath spriggitPath)
    {
        var origBranch = repo.Head;
        var branch = repo.CreateBranch($"Spriggit-Merge-Fix-{origBranch.Tip.Sha.Substring(0, 6)}", sha);
        Commands.Checkout(repo, branch);

        var meta = _externalMetaPersister.TryParseEmbeddedMeta(spriggitPath);
        Commands.Checkout(repo, origBranch);
        repo.Branches.Remove(branch);
        if (meta == null)
        {
            throw new ArgumentException($"Could not find spriggit meta for sha {sha}");
        }
        return meta;
    }

    private async Task ExecuteUpgrade(
        Repository repo,
        string oldSha,
        string newSha,
        DirectoryPath spriggitPath,
        DirectoryPath? dataFolder)
    {
        var origBranch = repo.Head;
        var newBranch = repo.CreateBranch($"Spriggit-Merge-Fix-New-{origBranch.Tip.Sha.Substring(0, 6)}", newSha);
        Commands.Checkout(repo, newBranch);
        
        var newMeta = await _getMetaToUse.Get(null, spriggitPath, CancellationToken.None);
        var newEntryPoint = await _entryPointCache.GetFor(newMeta.ToMeta(), CancellationToken.None);
        if (newEntryPoint == null)
        {
            throw new NullReferenceException($"Could not construct entry point for {newMeta}");
        }

        var newExternalMeta = _externalMetaPersister.TryParseEmbeddedMeta(spriggitPath);
        if (newExternalMeta == null)
        {
            throw new NullReferenceException($"Could not find embedded meta on {newSha}");
        }
        
        var oldBranch = repo.CreateBranch($"Spriggit-Merge-Fix-Old-{origBranch.Tip.Sha.Substring(0, 6)}", oldSha);
        Commands.Checkout(repo, oldBranch);
        
        var oldMeta = await _getMetaToUse.Get(null, spriggitPath, CancellationToken.None);
        var oldEntryPoint = await _entryPointCache.GetFor(oldMeta.ToMeta(), CancellationToken.None);
        if (oldEntryPoint == null)
        {
            throw new NullReferenceException($"Could not construct entry point for {oldMeta}");
        }

        var oldExternalMeta = _externalMetaPersister.TryParseEmbeddedMeta(spriggitPath);
        if (oldExternalMeta == null)
        {
            throw new NullReferenceException($"Could not find embedded meta on {oldSha}");
        }

        if (newExternalMeta.Release != oldExternalMeta.Release)
        {
            throw new ArgumentException($"External meta releases did not match {newExternalMeta.Release} != {oldExternalMeta.Release}");
        }

        if (newExternalMeta.ModKey != oldExternalMeta.ModKey)
        {
            throw new ArgumentException($"External meta ModKeys did not match {newExternalMeta.ModKey} != {oldExternalMeta.ModKey}");
        }
        
        using var tmp = TempFolder.Factory();

        var modPath = Path.Combine(tmp.Dir.Path, oldExternalMeta.ModKey.FileName);
        
        await oldEntryPoint.Deserialize(
            inputPath: spriggitPath, outputPath: modPath, dataPath: dataFolder,
            knownMasters: Array.Empty<KnownMaster>(),
            workDropoff: null, fileSystem: null, streamCreator: null, cancel: CancellationToken.None);

        await newEntryPoint.Serialize(modPath, spriggitPath, dataFolder,
            knownMasters: Array.Empty<KnownMaster>(),
            newExternalMeta.Release,
            null, null, null, newExternalMeta.Source, cancel: CancellationToken.None);
    }
}