using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Shouldly;
using Xunit;
using StarfieldEntryPoint = Spriggit.Yaml.Starfield.EntryPoint;

namespace Spriggit.Tests.Sort.SerializationSorting.Starfield;

/// <summary>
/// Tests that verify ImpactDataSet sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Starfield
/// </summary>
public class ImpactDataSetTests
{
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldImpactDataSet_SortsCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath existingTempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create a new ImpactDataSet with impacts
        var impactDataSet = mod.ImpactDataSets.AddNew("TestImpactDataSet");

        // Add impacts in unsorted order (should sort by Material, then Impact)
        impactDataSet.Impacts.Add(new Mutagen.Bethesda.Starfield.ImpactData()
        {
            Material = new FormKey(mod.ModKey, 0x789).ToLink<Mutagen.Bethesda.Starfield.IMaterialTypeGetter>(),
            Impact = new FormKey(mod.ModKey, 0x456).ToLink<Mutagen.Bethesda.Starfield.IImpactGetter>()
        });
        impactDataSet.Impacts.Add(new Mutagen.Bethesda.Starfield.ImpactData()
        {
            Material = new FormKey(mod.ModKey, 0x123).ToLink<Mutagen.Bethesda.Starfield.IMaterialTypeGetter>(),
            Impact = new FormKey(mod.ModKey, 0xABC).ToLink<Mutagen.Bethesda.Starfield.IImpactGetter>()
        });
        impactDataSet.Impacts.Add(new Mutagen.Bethesda.Starfield.ImpactData()
        {
            Material = new FormKey(mod.ModKey, 0x456).ToLink<Mutagen.Bethesda.Starfield.IMaterialTypeGetter>(),
            Impact = new FormKey(mod.ModKey, 0x123).ToLink<Mutagen.Bethesda.Starfield.IImpactGetter>()
        });

        // Verify initial order is NOT sorted
        var initialMaterials = impactDataSet.Impacts.Select(i => i.Material.FormKey.ID).ToArray();
        initialMaterials.ShouldBe(new uint[] { 0x789, 0x123, 0x456 });

        // Also verify we have the right number of impacts
        impactDataSet.Impacts.Count.ShouldBe(3);

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, existingTempDir, entryPoint, fileSystem);

        // Verify sorting worked correctly
        var deserializedImpactDataSet = deserializedMod.ImpactDataSets.First();
        var sortedMaterials = deserializedImpactDataSet.Impacts?.Select(i => i.Material.FormKey.ID).ToArray();

        // First verify we still have all the impacts
        deserializedImpactDataSet.Impacts.ShouldNotBeNull();
        deserializedImpactDataSet.Impacts.Count.ShouldBe(3);

        // Impacts should now be sorted by Material FormKey ID
        sortedMaterials.ShouldBe(new uint[] { 0x123, 0x456, 0x789 });
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldImpactDataSet_SortsByMaterialThenImpact_ThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath existingTempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create a new ImpactDataSet with multiple impacts for same material
        var impactDataSet = mod.ImpactDataSets.AddNew("TestImpactDataSet");

        // Add impacts where same material has different impacts - should sort by Material first, then Impact
        impactDataSet.Impacts.Add(new Mutagen.Bethesda.Starfield.ImpactData()
        {
            Material = new FormKey(mod.ModKey, 0x456).ToLink<Mutagen.Bethesda.Starfield.IMaterialTypeGetter>(),
            Impact = new FormKey(mod.ModKey, 0x999).ToLink<Mutagen.Bethesda.Starfield.IImpactGetter>()
        });
        impactDataSet.Impacts.Add(new Mutagen.Bethesda.Starfield.ImpactData()
        {
            Material = new FormKey(mod.ModKey, 0x123).ToLink<Mutagen.Bethesda.Starfield.IMaterialTypeGetter>(),
            Impact = new FormKey(mod.ModKey, 0x789).ToLink<Mutagen.Bethesda.Starfield.IImpactGetter>()
        });
        impactDataSet.Impacts.Add(new Mutagen.Bethesda.Starfield.ImpactData()
        {
            Material = new FormKey(mod.ModKey, 0x123).ToLink<Mutagen.Bethesda.Starfield.IMaterialTypeGetter>(),
            Impact = new FormKey(mod.ModKey, 0x456).ToLink<Mutagen.Bethesda.Starfield.IImpactGetter>()
        });

        // Verify initial order is NOT sorted
        var initialPairs = impactDataSet.Impacts.Select(i => $"{i.Material.FormKey.ID:X3},{i.Impact.FormKey.ID:X3}").ToArray();
        initialPairs.ShouldBe(new[] { "456,999", "123,789", "123,456" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, existingTempDir, entryPoint, fileSystem);

        // Verify sorting worked correctly
        var deserializedImpactDataSet = deserializedMod.ImpactDataSets.First();
        var sortedPairs = deserializedImpactDataSet.Impacts?.Select(i => $"{i.Material.FormKey.ID:X3},{i.Impact.FormKey.ID:X3}").ToArray();

        // First verify we still have all the impacts
        deserializedImpactDataSet.Impacts.ShouldNotBeNull();
        deserializedImpactDataSet.Impacts.Count.ShouldBe(3);

        // Impacts should now be sorted by Material, then Impact
        sortedPairs.ShouldBe(new[] { "123,456", "123,789", "456,999" });
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldImpactDataSet_NullImpacts_HandledCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath existingTempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create an ImpactDataSet with null Impacts (should remain null)
        var impactDataSet = mod.ImpactDataSets.AddNew("TestImpactDataSet");
        // Don't set Impacts - leave it null

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, existingTempDir, entryPoint, fileSystem);

        var deserializedImpactDataSet = deserializedMod.ImpactDataSets.First();

        // Should remain null or empty
        (deserializedImpactDataSet.Impacts == null ||
         deserializedImpactDataSet.Impacts.Count == 0).ShouldBeTrue();
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldImpactDataSet_SingleImpact_NoSortingNeededThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath existingTempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create an ImpactDataSet with single impact (no sorting needed)
        var impactDataSet = mod.ImpactDataSets.AddNew("TestImpactDataSet");

        // Add single impact
        impactDataSet.Impacts.Add(new Mutagen.Bethesda.Starfield.ImpactData()
        {
            Material = new FormKey(mod.ModKey, 0x789).ToLink<Mutagen.Bethesda.Starfield.IMaterialTypeGetter>(),
            Impact = new FormKey(mod.ModKey, 0x456).ToLink<Mutagen.Bethesda.Starfield.IImpactGetter>()
        });

        // Verify we have exactly one impact
        impactDataSet.Impacts.Count.ShouldBe(1);

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, existingTempDir, entryPoint, fileSystem);

        var deserializedImpactDataSet = deserializedMod.ImpactDataSets.First();

        // Should still have exactly one impact
        deserializedImpactDataSet.Impacts.ShouldNotBeNull();
        deserializedImpactDataSet.Impacts.Count.ShouldBe(1);

        // Verify the impact data is preserved
        var impact = deserializedImpactDataSet.Impacts.First();
        impact.Material.FormKey.ID.ShouldBe((uint)0x789);
        impact.Impact.FormKey.ID.ShouldBe((uint)0x456);
    }
}