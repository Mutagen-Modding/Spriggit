using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Shouldly;
using Spriggit.Core;
using Xunit;
using SkyrimEntryPoint = Spriggit.Yaml.Skyrim.EntryPoint;

namespace Spriggit.Tests.Sort.SerializationSorting.Skyrim;

/// <summary>
/// Tests that verify Race sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Skyrim
/// </summary>
public class RaceCustomizationTests
{
    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimRace_SortsMovementTypeNames(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create a race with out-of-order movement type names
        var race = mod.Races.AddNew("TestRace");

        // Add movement type names in unsorted order
        race.MovementTypeNames.Add("Walk");
        race.MovementTypeNames.Add("Run");
        race.MovementTypeNames.Add("Sprint");
        race.MovementTypeNames.Add("Crouch");
        race.MovementTypeNames.Add("Swim");
        race.MovementTypeNames.Add("Fly");

        // Verify initial order is NOT sorted
        var initialNames = race.MovementTypeNames.ToArray();
        initialNames.ShouldBe(new[] { "Walk", "Run", "Sprint", "Crouch", "Swim", "Fly" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedRace = deserializedMod.Races.First();
        var sortedNames = deserializedRace.MovementTypeNames.ToArray();

        // MovementTypeNames should now be sorted alphabetically
        sortedNames.ShouldBe(new[] { "Crouch", "Fly", "Run", "Sprint", "Swim", "Walk" });
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimRace_SortsAttacks(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create a race with out-of-order attacks
        var race = mod.Races.AddNew("TestRace");

        // Add attacks in unsorted order
        race.Attacks.Add(new Attack { AttackEvent = "RightPowerAttack" });
        race.Attacks.Add(new Attack { AttackEvent = "Attack" });
        race.Attacks.Add(new Attack { AttackEvent = "LeftAttack" });

        // Verify initial order is NOT sorted
        var initialAttacks = race.Attacks.Select(a => a.AttackEvent).ToArray();
        initialAttacks.ShouldBe(new[] { "RightPowerAttack", "Attack", "LeftAttack" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedRace = deserializedMod.Races.First();
        var sortedAttacks = deserializedRace.Attacks.Select(a => a.AttackEvent).ToArray();

        // Attacks should now be sorted by AttackEvent
        sortedAttacks.ShouldBe(new[] { "Attack", "LeftAttack", "RightPowerAttack" });
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimRace_SortsBothMovementTypesAndAttacks(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create a race with both out-of-order movement types and attacks
        var race = mod.Races.AddNew("TestRace");

        // Add movement type names in unsorted order
        race.MovementTypeNames.Add("Omega");
        race.MovementTypeNames.Add("Alpha");
        race.MovementTypeNames.Add("Beta");

        // Add attacks in unsorted order
        race.Attacks.Add(new Attack { AttackEvent = "Zeta" });
        race.Attacks.Add(new Attack { AttackEvent = "Delta" });
        race.Attacks.Add(new Attack { AttackEvent = "Epsilon" });

        // Verify initial order is NOT sorted
        var initialNames = race.MovementTypeNames.ToArray();
        initialNames.ShouldBe(new[] { "Omega", "Alpha", "Beta" });
        var initialAttacks = race.Attacks.Select(a => a.AttackEvent).ToArray();
        initialAttacks.ShouldBe(new[] { "Zeta", "Delta", "Epsilon" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedRace = deserializedMod.Races.First();

        // Both lists should now be sorted
        var sortedNames = deserializedRace.MovementTypeNames.ToArray();
        sortedNames.ShouldBe(new[] { "Alpha", "Beta", "Omega" });

        var sortedAttacks = deserializedRace.Attacks.Select(a => a.AttackEvent).ToArray();
        sortedAttacks.ShouldBe(new[] { "Delta", "Epsilon", "Zeta" });
    }
}
