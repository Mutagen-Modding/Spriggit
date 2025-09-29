using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Shouldly;
using Spriggit.Core;
using Xunit;
using Fallout4EntryPoint = Spriggit.Yaml.Fallout4.EntryPoint;

namespace Spriggit.Tests.Sort.SerializationSorting.Fallout4;

/// <summary>
/// Tests that verify Perk sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Fallout4
/// </summary>
public class PerkTests
{
    [Theory, MutagenModAutoData(GameRelease.Fallout4)]
    public async Task Fallout4Perk_SortsCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        Fallout4EntryPoint entryPoint,
        Fallout4Mod mod)
    {
        // Create a perk with out-of-order effects
        var perk = mod.Perks.AddNew("TestPerk");
        perk.Effects.Add(new PerkAbilityEffect()
        {
            Priority = 5,
        });
        perk.Effects.Add(new PerkAbilityEffect()
        {
            Priority = 4,
        });
        perk.Effects.Add(new PerkAbilityEffect()
        {
            Priority = 7,
        });

        // Verify initial order is NOT sorted
        var initialPriorities = perk.Effects.Select(e => e.Priority).ToList();
        initialPriorities.ShouldBe(new[] { (byte)5, (byte)4, (byte)7 });

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

        // Read the deserialized mod and verify sorting
        var deserializedMod = Fallout4Mod.CreateFromBinary(deserializedModPath, Fallout4Release.Fallout4, new BinaryReadParameters()
        {
            FileSystem = fileSystem
        });

        var deserializedPerk = deserializedMod.Perks.First();
        var sortedPriorities = deserializedPerk.Effects.Select(e => e.Priority).ToList();

        // Effects should now be sorted by Priority
        sortedPriorities.ShouldBe(new[] { (byte)4, (byte)5, (byte)7 });
    }
}