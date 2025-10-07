using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Starfield;
using Noggog;
using Spriggit.Core;
using SkyrimEntryPoint = Spriggit.Yaml.Skyrim.EntryPoint;
using Fallout4EntryPoint = Spriggit.Yaml.Fallout4.EntryPoint;
using StarfieldEntryPoint = Spriggit.Yaml.Starfield.EntryPoint;

namespace Spriggit.Tests.Sort.SerializationSorting;

/// <summary>
/// Shared helper methods for serialization sorting tests across all games
/// </summary>
public static class SerializationTestHelper
{
    /// <summary>
    /// Helper method to perform the full serialization -> deserialization cycle for Skyrim testing
    /// </summary>
    public static async Task<SkyrimMod> SerializeAndDeserialize(
        SkyrimMod mod,
        DirectoryPath tempDir,
        SkyrimEntryPoint entryPoint,
        IFileSystem fileSystem)
    {
        // Paths for serialization cycle
        var modPath = Path.Combine(tempDir.Path, mod.ModKey.FileName);
        var spriggitPath = Path.Combine(tempDir.Path, "spriggit");
        var deserializedModPath = Path.Combine(tempDir.Path, "deserialized.esp");

        // Create directories and write original mod to disk
        fileSystem.Directory.CreateDirectory(tempDir);
        mod.WriteToBinary(modPath, new BinaryWriteParameters()
        {
            FileSystem = fileSystem
        });

        // Serialize with Spriggit
        await entryPoint.Serialize(
            modPath: modPath,
            outputDir: spriggitPath,
            dataPath: null,
            knownMasters: [],
            release: GameRelease.SkyrimSE,
            fileSystem: fileSystem,
            workDropoff: null,
            streamCreator: null,
            cancel: CancellationToken.None,
            meta: new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Skyrim",
                Version = "1.0.0"
            },
            throwOnUnknown: false);

        // Deserialize back to mod file
        await entryPoint.Deserialize(
            inputPath: spriggitPath,
            outputPath: deserializedModPath,
            dataPath: null,
            knownMasters: [],
            fileSystem: fileSystem,
            workDropoff: null,
            streamCreator: null,
            cancel: CancellationToken.None);

        // Read and return the deserialized mod
        return SkyrimMod.CreateFromBinary(deserializedModPath, SkyrimRelease.SkyrimSE, new BinaryReadParameters()
        {
            FileSystem = fileSystem
        });
    }

    /// <summary>
    /// Helper method to perform the full serialization -> deserialization cycle for Fallout4 testing
    /// </summary>
    public static async Task<Fallout4Mod> SerializeAndDeserialize(
        Fallout4Mod mod,
        DirectoryPath tempDir,
        Fallout4EntryPoint entryPoint,
        IFileSystem fileSystem)
    {
        // Paths for serialization cycle
        var modPath = Path.Combine(tempDir.Path, mod.ModKey.FileName);
        var spriggitPath = Path.Combine(tempDir.Path, "spriggit");
        var deserializedModPath = Path.Combine(tempDir.Path, "deserialized.esp");

        // Create directories and write original mod to disk
        fileSystem.Directory.CreateDirectory(tempDir);
        mod.WriteToBinary(modPath, new BinaryWriteParameters()
        {
            FileSystem = fileSystem
        });

        // Serialize with Spriggit
        await entryPoint.Serialize(
            modPath: modPath,
            outputDir: spriggitPath,
            dataPath: null,
            knownMasters: [],
            release: GameRelease.Fallout4,
            fileSystem: fileSystem,
            workDropoff: null,
            streamCreator: null,
            cancel: CancellationToken.None,
            meta: new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Fallout4",
                Version = "1.0.0"
            },
            throwOnUnknown: false);

        // Deserialize back to mod file
        await entryPoint.Deserialize(
            inputPath: spriggitPath,
            outputPath: deserializedModPath,
            dataPath: null,
            knownMasters: [],
            fileSystem: fileSystem,
            workDropoff: null,
            streamCreator: null,
            cancel: CancellationToken.None);

        // Read and return the deserialized mod
        return Fallout4Mod.CreateFromBinary(deserializedModPath, Fallout4Release.Fallout4, new BinaryReadParameters()
        {
            FileSystem = fileSystem
        });
    }

    /// <summary>
    /// Helper method to perform the full serialization -> deserialization cycle for Starfield testing
    /// </summary>
    public static async Task<StarfieldMod> SerializeAndDeserialize(
        StarfieldMod mod,
        DirectoryPath tempDir,
        StarfieldEntryPoint entryPoint,
        IFileSystem fileSystem)
    {
        // Paths for serialization cycle
        var modPath = Path.Combine(tempDir.Path, mod.ModKey.FileName);
        var spriggitPath = Path.Combine(tempDir.Path, "spriggit");
        var deserializedModPath = Path.Combine(tempDir.Path, "deserialized.esm");

        // Create directories and write original mod to disk
        fileSystem.Directory.CreateDirectory(tempDir);
        mod.WriteToBinary(modPath, new BinaryWriteParameters()
        {
            FileSystem = fileSystem
        });

        // Serialize with Spriggit
        await entryPoint.Serialize(
            modPath: modPath,
            outputDir: spriggitPath,
            dataPath: null,
            knownMasters: [],
            release: GameRelease.Starfield,
            fileSystem: fileSystem,
            workDropoff: null,
            streamCreator: null,
            cancel: CancellationToken.None,
            meta: new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Starfield",
                Version = "1.0.0"
            },
            throwOnUnknown: false);

        // Deserialize back to mod file
        await entryPoint.Deserialize(
            inputPath: spriggitPath,
            outputPath: deserializedModPath,
            dataPath: null,
            knownMasters: [],
            fileSystem: fileSystem,
            workDropoff: null,
            streamCreator: null,
            cancel: CancellationToken.None);

        // Read and return the deserialized mod
        return StarfieldMod.CreateFromBinary(deserializedModPath, StarfieldRelease.Starfield, new BinaryReadParameters()
        {
            FileSystem = fileSystem
        });
    }
}