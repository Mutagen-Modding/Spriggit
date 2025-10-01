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
/// Tests that verify Race sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Fallout4
/// </summary>
public class RaceCustomizationTests
{
    [Theory, MutagenModAutoData(GameRelease.Fallout4)]
    public async Task Fallout4Race_SortsMovementTypeNames(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        Fallout4EntryPoint entryPoint,
        Fallout4Mod mod)
    {
        // Create a race with out-of-order movement type names
        var race = mod.Races.AddNew("TestRace");

        // Add movement type names in unsorted order
        race.MovementTypeNames.Add("Walk");
        race.MovementTypeNames.Add("Run");
        race.MovementTypeNames.Add("Sprint");
        race.MovementTypeNames.Add("Crouch");
        race.MovementTypeNames.Add("Swim");

        // Verify initial order is NOT sorted
        var initialNames = race.MovementTypeNames.ToArray();
        initialNames.ShouldBe(new[] { "Walk", "Run", "Sprint", "Crouch", "Swim" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedRace = deserializedMod.Races.First();
        var sortedNames = deserializedRace.MovementTypeNames.ToArray();

        // MovementTypeNames should now be sorted alphabetically
        sortedNames.ShouldBe(new[] { "Crouch", "Run", "Sprint", "Swim", "Walk" });
    }

    [Theory, MutagenModAutoData(GameRelease.Fallout4)]
    public async Task Fallout4Race_SortsAttacks(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        Fallout4EntryPoint entryPoint,
        Fallout4Mod mod)
    {
        // Create a race with out-of-order attacks
        var race = mod.Races.AddNew("TestRace");

        // Add attacks in unsorted order
        race.Attacks.Add(new Attack { AttackEvent = "PowerAttack" });
        race.Attacks.Add(new Attack { AttackEvent = "Attack" });
        race.Attacks.Add(new Attack { AttackEvent = "Bash" });
        race.Attacks.Add(new Attack { AttackEvent = "Special" });

        // Verify initial order is NOT sorted
        var initialAttacks = race.Attacks.Select(a => a.AttackEvent).ToArray();
        initialAttacks.ShouldBe(new[] { "PowerAttack", "Attack", "Bash", "Special" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedRace = deserializedMod.Races.First();
        var sortedAttacks = deserializedRace.Attacks.Select(a => a.AttackEvent).ToArray();

        // Attacks should now be sorted by AttackEvent
        sortedAttacks.ShouldBe(new[] { "Attack", "Bash", "PowerAttack", "Special" });
    }

    [Theory, MutagenModAutoData(GameRelease.Fallout4)]
    public async Task Fallout4Race_SortsBothMovementTypesAndAttacks(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        Fallout4EntryPoint entryPoint,
        Fallout4Mod mod)
    {
        // Create a race with both out-of-order movement types and attacks
        var race = mod.Races.AddNew("TestRace");

        // Add movement type names in unsorted order
        race.MovementTypeNames.Add("Zebra");
        race.MovementTypeNames.Add("Apple");

        // Add attacks in unsorted order
        race.Attacks.Add(new Attack { AttackEvent = "Charlie" });
        race.Attacks.Add(new Attack { AttackEvent = "Alpha" });

        // Verify initial order is NOT sorted
        var initialNames = race.MovementTypeNames.ToArray();
        initialNames.ShouldBe(new[] { "Zebra", "Apple" });
        var initialAttacks = race.Attacks.Select(a => a.AttackEvent).ToArray();
        initialAttacks.ShouldBe(new[] { "Charlie", "Alpha" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedRace = deserializedMod.Races.First();

        // Both lists should now be sorted
        var sortedNames = deserializedRace.MovementTypeNames.ToArray();
        sortedNames.ShouldBe(new[] { "Apple", "Zebra" });

        var sortedAttacks = deserializedRace.Attacks.Select(a => a.AttackEvent).ToArray();
        sortedAttacks.ShouldBe(new[] { "Alpha", "Charlie" });
    }
}
