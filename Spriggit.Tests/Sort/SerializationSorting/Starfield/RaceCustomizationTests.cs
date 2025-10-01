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
/// Tests that verify Race sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Starfield
/// </summary>
public class RaceCustomizationTests
{
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldRace_SortsMovementTypeNames(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create a race with out-of-order movement type names
        var race = mod.Races.AddNew("TestRace");

        // Add movement type names in unsorted order
        race.MovementTypeNames.Add("Walk");
        race.MovementTypeNames.Add("Run");
        race.MovementTypeNames.Add("Sprint");
        race.MovementTypeNames.Add("Crouch");

        // Verify initial order is NOT sorted
        var initialNames = race.MovementTypeNames.ToArray();
        initialNames.ShouldBe(new[] { "Walk", "Run", "Sprint", "Crouch" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedRace = deserializedMod.Races.First();
        var sortedNames = deserializedRace.MovementTypeNames.ToArray();

        // MovementTypeNames should now be sorted alphabetically
        sortedNames.ShouldBe(new[] { "Crouch", "Run", "Sprint", "Walk" });
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldRace_SortsAttacks(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create a race with out-of-order attacks
        var race = mod.Races.AddNew("TestRace");

        // Add attacks in unsorted order
        race.Attacks.Add(new Attack { AttackEvent = "ZEvent" });
        race.Attacks.Add(new Attack { AttackEvent = "AEvent" });
        race.Attacks.Add(new Attack { AttackEvent = "MEvent" });

        // Verify initial order is NOT sorted
        var initialAttacks = race.Attacks.Select(a => a.AttackEvent).ToArray();
        initialAttacks.ShouldBe(new[] { "ZEvent", "AEvent", "MEvent" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedRace = deserializedMod.Races.First();
        var sortedAttacks = deserializedRace.Attacks.Select(a => a.AttackEvent).ToArray();

        // Attacks should now be sorted by AttackEvent
        sortedAttacks.ShouldBe(new[] { "AEvent", "MEvent", "ZEvent" });
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldRace_SortsBothMovementTypesAndAttacks(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create a race with both out-of-order movement types and attacks
        var race = mod.Races.AddNew("TestRace");

        // Add movement type names in unsorted order
        race.MovementTypeNames.Add("Zulu");
        race.MovementTypeNames.Add("Alpha");
        race.MovementTypeNames.Add("Mike");

        // Add attacks in unsorted order
        race.Attacks.Add(new Attack { AttackEvent = "Gamma" });
        race.Attacks.Add(new Attack { AttackEvent = "Beta" });
        race.Attacks.Add(new Attack { AttackEvent = "Alpha" });

        // Verify initial order is NOT sorted
        var initialNames = race.MovementTypeNames.ToArray();
        initialNames.ShouldBe(new[] { "Zulu", "Alpha", "Mike" });
        var initialAttacks = race.Attacks.Select(a => a.AttackEvent).ToArray();
        initialAttacks.ShouldBe(new[] { "Gamma", "Beta", "Alpha" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedRace = deserializedMod.Races.First();

        // Both lists should now be sorted
        var sortedNames = deserializedRace.MovementTypeNames.ToArray();
        sortedNames.ShouldBe(new[] { "Alpha", "Mike", "Zulu" });

        var sortedAttacks = deserializedRace.Attacks.Select(a => a.AttackEvent).ToArray();
        sortedAttacks.ShouldBe(new[] { "Alpha", "Beta", "Gamma" });
    }
}
