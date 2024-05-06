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
    private readonly FormIDCollisionDetector _detector;

    public FormIDCollisionFixer(
        IFileSystem fileSystem,
        FormIDCollisionDetector detector)
    {
        _fileSystem = fileSystem;
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