using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Oblivion;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Noggog;
using Spriggit.Core;
using Spriggit.Yaml.Oblivion;

namespace Spriggit.Tests;

public class TestOblivionUtil
{
    public static async Task<IOblivionModDisposableGetter> PassThrough(
        IFileSystem fileSystem,
        OblivionMod mod, 
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
        OblivionMod mod, 
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
            release: GameRelease.Oblivion, 
            workDropoff: null, fileSystem: fileSystem,
            knownMasters: Array.Empty<KnownMaster>(),
            streamCreator: null, meta: new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Oblivion",
                Version = "Test"
            }, cancel: CancellationToken.None);
    }
    
    public static async Task<IOblivionModDisposableGetter> Import(
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
            knownMasters: Array.Empty<KnownMaster>(),
            fileSystem: fileSystem,
            streamCreator: null, cancel: CancellationToken.None);
        var reimport = OblivionMod.CreateFromBinaryOverlay(modPath2,
            BinaryReadParameters.Default with
            {
                FileSystem = fileSystem
            });
        return reimport;
    }
}