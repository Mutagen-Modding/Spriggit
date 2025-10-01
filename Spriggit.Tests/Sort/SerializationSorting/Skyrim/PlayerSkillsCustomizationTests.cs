using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Shouldly;
using Spriggit.Core;
using Xunit;
using SkyrimEntryPoint = Spriggit.Yaml.Skyrim.EntryPoint;

namespace Spriggit.Tests.Sort.SerializationSorting.Skyrim;

/// <summary>
/// Tests that verify PlayerSkills omit functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Skyrim
/// </summary>
public class PlayerSkillsCustomizationTests
{
    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimPlayerSkills_OmitsUnusedFields(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create an NPC with PlayerSkills data
        var npc = mod.Npcs.AddNew("TestNpc");
        var playerSkills = new PlayerSkills
        {
            Health = 100,
            Magicka = 50,
            Stamina = 75,
            Unused = 12345, // Should be omitted
            FarAwayModelDistance = 4000.0f,
            GearedUpWeapons = 3,
            Unused2 = new byte[] { 1, 2, 3 } // Should be omitted
        };
        npc.PlayerSkills = playerSkills;

        // Verify initial values are set
        npc.PlayerSkills.Unused.ShouldBe((ushort)12345);
        npc.PlayerSkills.Unused2.ShouldBe(new byte[] { 1, 2, 3 });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedNpc = deserializedMod.Npcs.First();

        // PlayerSkills should exist
        deserializedNpc.PlayerSkills.ShouldNotBeNull();

        // Non-omitted fields should be preserved
        deserializedNpc.PlayerSkills.Health.ShouldBe((ushort)100);
        deserializedNpc.PlayerSkills.Magicka.ShouldBe((ushort)50);
        deserializedNpc.PlayerSkills.Stamina.ShouldBe((ushort)75);
        deserializedNpc.PlayerSkills.FarAwayModelDistance.ShouldBe(4000.0f);
        deserializedNpc.PlayerSkills.GearedUpWeapons.ShouldBe((byte)3);

        // Omitted fields should be default values (0 or empty)
        deserializedNpc.PlayerSkills.Unused.ShouldBe((ushort)0);
        deserializedNpc.PlayerSkills.Unused2.ShouldBe(new byte[] { 0, 0, 0 });
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimPlayerSkills_WithSkillValues_PreservesNonOmittedData(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create an NPC with PlayerSkills including skill values
        var npc = mod.Npcs.AddNew("TestNpc");
        var playerSkills = new PlayerSkills
        {
            Health = 150,
            Magicka = 100,
            Stamina = 125,
            Unused = 65535, // Should be omitted
            FarAwayModelDistance = 5000.0f,
            GearedUpWeapons = 5,
            Unused2 = new byte[] { 255, 255, 255 } // Should be omitted
        };

        // Add some skill values
        playerSkills.SkillValues[Skill.OneHanded] = 50;
        playerSkills.SkillValues[Skill.TwoHanded] = 75;
        playerSkills.SkillOffsets[Skill.Archery] = 10;

        npc.PlayerSkills = playerSkills;

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedNpc = deserializedMod.Npcs.First();

        // All non-omitted data should be preserved
        deserializedNpc.PlayerSkills.ShouldNotBeNull();
        deserializedNpc.PlayerSkills.Health.ShouldBe((ushort)150);
        deserializedNpc.PlayerSkills.Magicka.ShouldBe((ushort)100);
        deserializedNpc.PlayerSkills.Stamina.ShouldBe((ushort)125);
        deserializedNpc.PlayerSkills.FarAwayModelDistance.ShouldBe(5000.0f);
        deserializedNpc.PlayerSkills.GearedUpWeapons.ShouldBe((byte)5);

        // Skill values should be preserved
        deserializedNpc.PlayerSkills.SkillValues[Skill.OneHanded].ShouldBe((byte)50);
        deserializedNpc.PlayerSkills.SkillValues[Skill.TwoHanded].ShouldBe((byte)75);
        deserializedNpc.PlayerSkills.SkillOffsets[Skill.Archery].ShouldBe((byte)10);

        // Omitted fields should be default values
        deserializedNpc.PlayerSkills.Unused.ShouldBe((ushort)0);
        deserializedNpc.PlayerSkills.Unused2.ShouldBe(new byte[] { 0, 0, 0 });
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimPlayerSkills_NullPlayerSkills_NoError(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create an NPC without PlayerSkills data
        var npc = mod.Npcs.AddNew("TestNpc");
        // Don't set PlayerSkills - leave it null

        // Perform serialization cycle and get deserialized mod - should not throw
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedNpc = deserializedMod.Npcs.First();
        deserializedNpc.ShouldNotBeNull();
    }
}
