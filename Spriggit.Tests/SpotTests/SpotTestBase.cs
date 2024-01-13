using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Starfield;
using Noggog;
using Spriggit.Core;
using Spriggit.Yaml.Starfield;

namespace Spriggit.Tests.SpotTests;

public class SpotTestBase
{
    public static async Task<IStarfieldModDisposableGetter> PassThrough(
        IFileSystem fileSystem,
        StarfieldMod mod, 
        DirectoryPath dataFolder,
        DirectoryPath spriggitFolder,
        ModKey otherModKey,
        EntryPoint entryPoint)
    {
        var modPath = new ModPath(Path.Combine(dataFolder, mod.ModKey.ToString()));
        fileSystem.Directory.CreateDirectory(dataFolder);
        mod.WriteToBinaryParallel(modPath, fileSystem: fileSystem);
        await entryPoint.Serialize(modPath, spriggitFolder, GameRelease.Starfield, workDropoff: null, fileSystem: fileSystem,
            streamCreator: null, new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Starfield",
                Version = "Test"
            }, CancellationToken.None);
        var modPath2 = Path.Combine(dataFolder, otherModKey.ToString());
        await entryPoint.Deserialize(spriggitFolder, modPath2, workDropoff: null, fileSystem: fileSystem,
            streamCreator: null, CancellationToken.None);
        var reimport = StarfieldMod.CreateFromBinaryOverlay(modPath2, StarfieldRelease.Starfield, fileSystem: fileSystem);
        return reimport;
    }
}