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
/// Tests that verify Location sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Fallout4
/// </summary>
public class LocationTests
{

    [Theory, MutagenModAutoData(GameRelease.Fallout4)]
    public async Task Fallout4Location_SortsCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath existingTempDir,
        Fallout4EntryPoint entryPoint,
        Fallout4Mod mod)
    {
        // Create a new location to avoid FormKey conflicts
        var location = mod.Locations.AddNew("TestLocation");
        location.PersistentActorReferencesAdded = new Noggog.ExtendedList<Mutagen.Bethesda.Fallout4.PersistentActorReference>();

        // Add references in unsorted order (should sort by Grid.X, Grid.Y, Actor.FormKey, Location.FormKey)
        location.PersistentActorReferencesAdded.Add(new Mutagen.Bethesda.Fallout4.PersistentActorReference()
        {
            Grid = new P2Int16(2, 1),
            Actor = new FormKey(mod.ModKey, 0x123).ToLink<Mutagen.Bethesda.Fallout4.IPlacedNpcGetter>(),
            Location = new FormKey(mod.ModKey, 0x456).ToLink<Mutagen.Bethesda.Fallout4.IComplexLocationGetter>()
        });
        location.PersistentActorReferencesAdded.Add(new Mutagen.Bethesda.Fallout4.PersistentActorReference()
        {
            Grid = new P2Int16(1, 2),
            Actor = new FormKey(mod.ModKey, 0x789).ToLink<Mutagen.Bethesda.Fallout4.IPlacedNpcGetter>(),
            Location = new FormKey(mod.ModKey, 0x456).ToLink<Mutagen.Bethesda.Fallout4.IComplexLocationGetter>()
        });

        // Verify initial order is NOT sorted
        var initialGrids = location.PersistentActorReferencesAdded.Select(r => $"{r.Grid.X},{r.Grid.Y}").ToArray();
        initialGrids.ShouldBe(new[] { "2,1", "1,2" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, existingTempDir, entryPoint, fileSystem);

        // Verify sorting worked correctly
        var deserializedLocation = deserializedMod.Locations.First();
        var sortedGrids = deserializedLocation.PersistentActorReferencesAdded?.Select(r => $"{r.Grid.X},{r.Grid.Y}").ToArray();

        // References should now be sorted by Grid.X, then Grid.Y
        sortedGrids.ShouldBe(new[] { "1,2", "2,1" });
    }


    [Theory, MutagenModAutoData(GameRelease.Fallout4)]
    public async Task Fallout4Location_LocationRefTypeReferences_SortsCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath existingTempDir,
        Fallout4EntryPoint entryPoint,
        Fallout4Mod mod)
    {
        // Create a new location to avoid FormKey conflicts
        var location = mod.Locations.AddNew("TestLocation");
        location.LocationRefTypeReferencesAdded = new Noggog.ExtendedList<Mutagen.Bethesda.Fallout4.LocationRefTypeReference>();

        // Add references in unsorted order (should sort by Grid.X, Grid.Y, Ref.FormKey, Location.FormKey)
        location.LocationRefTypeReferencesAdded.Add(new Mutagen.Bethesda.Fallout4.LocationRefTypeReference()
        {
            Grid = new P2Int16(2, 1),
            Ref = new FormKey(mod.ModKey, 0x789).ToLink<Mutagen.Bethesda.Fallout4.IPlacedGetter>(),
            Location = new FormKey(mod.ModKey, 0x456).ToLink<Mutagen.Bethesda.Fallout4.IComplexLocationGetter>()
        });
        location.LocationRefTypeReferencesAdded.Add(new Mutagen.Bethesda.Fallout4.LocationRefTypeReference()
        {
            Grid = new P2Int16(1, 2),
            Ref = new FormKey(mod.ModKey, 0x123).ToLink<Mutagen.Bethesda.Fallout4.IPlacedGetter>(),
            Location = new FormKey(mod.ModKey, 0x456).ToLink<Mutagen.Bethesda.Fallout4.IComplexLocationGetter>()
        });

        // Verify initial order is NOT sorted
        var initialGrids = location.LocationRefTypeReferencesAdded.Select(r => $"{r.Grid.X},{r.Grid.Y}").ToArray();
        initialGrids.ShouldBe(new[] { "2,1", "1,2" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, existingTempDir, entryPoint, fileSystem);

        // Verify sorting worked correctly
        var deserializedLocation = deserializedMod.Locations.First();
        var sortedGrids = deserializedLocation.LocationRefTypeReferencesAdded?.Select(r => $"{r.Grid.X},{r.Grid.Y}").ToArray();

        // References should now be sorted by Grid.X, then Grid.Y
        sortedGrids.ShouldBe(new[] { "1,2", "2,1" });
    }

    [Theory, MutagenModAutoData(GameRelease.Fallout4)]
    public async Task Fallout4Location_EnableParentReferences_SortsCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath existingTempDir,
        Fallout4EntryPoint entryPoint,
        Fallout4Mod mod)
    {
        // Create a new location to avoid FormKey conflicts
        var location = mod.Locations.AddNew("TestLocation");
        location.EnableParentReferencesAdded = new Noggog.ExtendedList<Mutagen.Bethesda.Fallout4.EnableParentReference>();

        // Add references in unsorted order (should sort by Grid.X, Grid.Y, Ref.FormKey, Actor.FormKey)
        location.EnableParentReferencesAdded.Add(new Mutagen.Bethesda.Fallout4.EnableParentReference()
        {
            Grid = new P2Int16(2, 1),
            Ref = new FormKey(mod.ModKey, 0x789).ToLink<Mutagen.Bethesda.Fallout4.IPlacedGetter>(),
            Actor = new FormKey(mod.ModKey, 0x456).ToLink<Mutagen.Bethesda.Fallout4.IPlacedNpcGetter>()
        });
        location.EnableParentReferencesAdded.Add(new Mutagen.Bethesda.Fallout4.EnableParentReference()
        {
            Grid = new P2Int16(1, 2),
            Ref = new FormKey(mod.ModKey, 0x123).ToLink<Mutagen.Bethesda.Fallout4.IPlacedGetter>(),
            Actor = new FormKey(mod.ModKey, 0x456).ToLink<Mutagen.Bethesda.Fallout4.IPlacedNpcGetter>()
        });

        // Verify initial order is NOT sorted
        var initialGrids = location.EnableParentReferencesAdded.Select(r => $"{r.Grid.X},{r.Grid.Y}").ToArray();
        initialGrids.ShouldBe(new[] { "2,1", "1,2" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, existingTempDir, entryPoint, fileSystem);

        // Verify sorting worked correctly
        var deserializedLocation = deserializedMod.Locations.First();
        var sortedGrids = deserializedLocation.EnableParentReferencesAdded?.Select(r => $"{r.Grid.X},{r.Grid.Y}").ToArray();

        // References should now be sorted by Grid.X, then Grid.Y
        sortedGrids.ShouldBe(new[] { "1,2", "2,1" });
    }
}