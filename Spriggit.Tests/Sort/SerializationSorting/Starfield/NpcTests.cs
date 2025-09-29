using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Shouldly;
using Spriggit.Core;
using Xunit;
using StarfieldEntryPoint = Spriggit.Yaml.Starfield.EntryPoint;

namespace Spriggit.Tests.Sort.SerializationSorting.Starfield;

/// <summary>
/// Tests that verify NPC sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Starfield
/// </summary>
public class NpcTests
{
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldNpcMorphBlends_SortsCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create a new NPC to avoid FormKey conflicts
        var npc = mod.Npcs.AddNew("TestNPC");

        // Add morph blends in unsorted order (should sort by BlendName)
        npc.MorphBlends.Add(new NpcMorphBlend() { BlendName = "ZBlend" });
        npc.MorphBlends.Add(new NpcMorphBlend() { BlendName = "ABlend" });
        npc.MorphBlends.Add(new NpcMorphBlend() { BlendName = "MBlend" });

        // Verify initial order is NOT sorted
        var initialBlendNames = npc.MorphBlends.Select(mb => mb.BlendName).ToArray();
        initialBlendNames.ShouldBe(new[] { "ZBlend", "ABlend", "MBlend" });

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

        // Read the deserialized mod and verify sorting
        var deserializedMod = StarfieldMod.CreateFromBinary(deserializedModPath, StarfieldRelease.Starfield, new BinaryReadParameters()
        {
            FileSystem = fileSystem
        });

        var deserializedNpc = deserializedMod.Npcs.First();
        var sortedBlendNames = deserializedNpc.MorphBlends.Select(mb => mb.BlendName).ToArray();

        // Morph blends should now be sorted by BlendName
        sortedBlendNames.ShouldBe(new[] { "ABlend", "MBlend", "ZBlend" });
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldNpcFaceMorph_SortsCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create a new NPC with face morphs having out-of-order morph groups
        var npc = mod.Npcs.AddNew("TestNPC");
        npc.FaceMorphs.Add(new Mutagen.Bethesda.Starfield.NpcFaceMorph()
        {
            MorphGroups = new Noggog.ExtendedList<Mutagen.Bethesda.Starfield.NpcMorphGroup>()
            {
                new Mutagen.Bethesda.Starfield.NpcMorphGroup() { MorphGroup = "Xyz" },
                new Mutagen.Bethesda.Starfield.NpcMorphGroup() { MorphGroup = "Abc" },
                new Mutagen.Bethesda.Starfield.NpcMorphGroup() { MorphGroup = "Mno" }
            }
        });

        // Verify initial order is NOT sorted
        var initialMorphGroupNames = npc.FaceMorphs[0].MorphGroups.Select(mg => mg.MorphGroup).ToArray();
        initialMorphGroupNames.ShouldBe(new[] { "Xyz", "Abc", "Mno" });

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

        // Read the deserialized mod and verify sorting
        var deserializedMod = StarfieldMod.CreateFromBinary(deserializedModPath, StarfieldRelease.Starfield, new BinaryReadParameters()
        {
            FileSystem = fileSystem
        });

        var deserializedNpc = deserializedMod.Npcs.First();
        var sortedMorphGroupNames = deserializedNpc.FaceMorphs[0].MorphGroups.Select(mg => mg.MorphGroup).ToArray();

        // Morph groups should now be sorted by MorphGroup name
        sortedMorphGroupNames.ShouldBe(new[] { "Abc", "Mno", "Xyz" });
    }
}