using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Starfield;
using Noggog;
using Spriggit.Core;
using Spriggit.Yaml.Starfield;

namespace Spriggit.Tests;

public class TestUtil
{
    public static async Task<IStarfieldModDisposableGetter> PassThroughStarfield(
        IFileSystem fileSystem,
        StarfieldMod mod, 
        DirectoryPath dataFolder,
        DirectoryPath spriggitFolder,
        ModKey otherModKey,
        EntryPoint entryPoint)
    {
        await ExportStarfield(fileSystem, mod, dataFolder, spriggitFolder, entryPoint);
        return await ImportStarfield(fileSystem, otherModKey, dataFolder, spriggitFolder, entryPoint);
    }
    
    public static async Task ExportStarfield(
        IFileSystem fileSystem,
        StarfieldMod mod, 
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
        await entryPoint.Serialize(modPath, spriggitFolder, GameRelease.Starfield, workDropoff: null, fileSystem: fileSystem,
            streamCreator: null, new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Starfield",
                Version = "Test"
            }, CancellationToken.None);
    }
    
    public static async Task<IStarfieldModDisposableGetter> ImportStarfield(
        IFileSystem fileSystem,
        ModKey otherModKey, 
        DirectoryPath dataFolder,
        DirectoryPath spriggitFolder,
        EntryPoint entryPoint)
    {
        var modPath2 = Path.Combine(dataFolder, otherModKey.ToString());
        await entryPoint.Deserialize(spriggitFolder, modPath2, workDropoff: null, fileSystem: fileSystem,
            streamCreator: null, CancellationToken.None);
        var reimport = StarfieldMod.CreateFromBinaryOverlay(modPath2, StarfieldRelease.Starfield,
            BinaryReadParameters.Default with
            {
                FileSystem = fileSystem
            });
        return reimport;
    }
}