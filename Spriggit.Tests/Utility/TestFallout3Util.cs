using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout3;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Noggog;
using Spriggit.Core;
using Spriggit.Yaml.Fallout3;

namespace Spriggit.Tests.Utility;

public class TestFallout3Util
{
    public static async Task<IFallout3ModDisposableGetter> PassThrough(
        IFileSystem fileSystem,
        Fallout3Mod mod,
        DirectoryPath dataFolder,
        DirectoryPath spriggitFolder,
        ModKey otherModKey,
        EntryPoint entryPoint)
    {
        await Export(fileSystem, mod, dataFolder, spriggitFolder, entryPoint);
        return await Import(fileSystem, otherModKey, dataFolder, spriggitFolder, entryPoint);
    }

    public static async Task Export(
        IFileSystem fileSystem,
        Fallout3Mod mod,
        DirectoryPath dataFolder,
        DirectoryPath spriggitFolder,
        EntryPoint entryPoint)
    {
        var modPath = new ModPath(Path.Combine(dataFolder, mod.ModKey.ToString()));
        fileSystem.Directory.CreateDirectory(dataFolder);
        mod.WriteToBinary(modPath, new BinaryWriteParameters()
        {
            FileSystem = fileSystem
        });
        await entryPoint.Serialize(
            modPath: modPath, outputDir: spriggitFolder, dataPath: dataFolder,
            release: GameRelease.Fallout3,
            workDropoff: null, fileSystem: fileSystem,
            knownMasters: [],
            streamCreator: null, meta: new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Fallout3",
                Version = "Test"
            },
            throwOnUnknown: true,
            cancel: CancellationToken.None);
    }

    public static async Task<IFallout3ModDisposableGetter> Import(
        IFileSystem fileSystem,
        ModKey otherModKey,
        DirectoryPath dataFolder,
        DirectoryPath spriggitFolder,
        EntryPoint entryPoint)
    {
        var modPath2 = Path.Combine(dataFolder, otherModKey.ToString());
        await entryPoint.Deserialize(inputPath: spriggitFolder,
            outputPath: modPath2,
            dataPath: dataFolder,
            workDropoff: null,
            knownMasters: [],
            fileSystem: fileSystem,
            streamCreator: null, cancel: CancellationToken.None);
        var reimport = Fallout3Mod
            .Create(Fallout3Release.Fallout3)
            .FromPath(modPath2)
            .WithFileSystem(fileSystem)
            .Construct();
        return reimport;
    }
}
