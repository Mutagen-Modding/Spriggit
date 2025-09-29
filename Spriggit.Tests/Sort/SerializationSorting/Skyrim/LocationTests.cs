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
/// Tests that verify Location sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Skyrim
/// </summary>
public class LocationTests
{
    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimLocation_SortsCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create a new location to avoid FormKey conflicts
        var location = mod.Locations.AddNew("TestLocation");
        location.PersistentActorReferencesAdded = new Noggog.ExtendedList<Mutagen.Bethesda.Skyrim.PersistentActorReference>();

        // Add references in unsorted order (should sort by Grid.X, Grid.Y, Actor.FormKey, Location.FormKey)
        location.PersistentActorReferencesAdded.Add(new Mutagen.Bethesda.Skyrim.PersistentActorReference()
        {
            Grid = new P2Int16(5, 3),
            Actor = new FormKey(mod.ModKey, 0x123).ToLink<Mutagen.Bethesda.Skyrim.IPlacedNpcGetter>(),
            Location = new FormKey(mod.ModKey, 0x456).ToLink<Mutagen.Bethesda.Skyrim.IComplexLocationGetter>()
        });
        location.PersistentActorReferencesAdded.Add(new Mutagen.Bethesda.Skyrim.PersistentActorReference()
        {
            Grid = new P2Int16(1, 1),
            Actor = new FormKey(mod.ModKey, 0x789).ToLink<Mutagen.Bethesda.Skyrim.IPlacedNpcGetter>(),
            Location = new FormKey(mod.ModKey, 0x456).ToLink<Mutagen.Bethesda.Skyrim.IComplexLocationGetter>()
        });
        location.PersistentActorReferencesAdded.Add(new Mutagen.Bethesda.Skyrim.PersistentActorReference()
        {
            Grid = new P2Int16(3, 2),
            Actor = new FormKey(mod.ModKey, 0xABC).ToLink<Mutagen.Bethesda.Skyrim.IPlacedNpcGetter>(),
            Location = new FormKey(mod.ModKey, 0x456).ToLink<Mutagen.Bethesda.Skyrim.IComplexLocationGetter>()
        });

        // Verify initial order is NOT sorted
        var initialGrids = location.PersistentActorReferencesAdded.Select(r => $"{r.Grid.X},{r.Grid.Y}").ToArray();
        initialGrids.ShouldBe(new[] { "5,3", "1,1", "3,2" });

        // Also verify we have the right number of references
        location.PersistentActorReferencesAdded.Count.ShouldBe(3);

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

        // Read the deserialized mod and verify sorting
        var deserializedMod = SkyrimMod.CreateFromBinary(deserializedModPath, SkyrimRelease.SkyrimSE, new BinaryReadParameters()
        {
            FileSystem = fileSystem
        });

        var deserializedLocation = deserializedMod.Locations.First();
        var sortedGrids = deserializedLocation.PersistentActorReferencesAdded?.Select(r => $"{r.Grid.X},{r.Grid.Y}").ToArray();

        // First verify we still have all the references
        deserializedLocation.PersistentActorReferencesAdded.ShouldNotBeNull();
        deserializedLocation.PersistentActorReferencesAdded.Count.ShouldBe(3);

        // References should now be sorted by Grid.X, then Grid.Y
        sortedGrids.ShouldBe(new[] { "1,1", "3,2", "5,3" });
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimLocation_NullReferences_HandledCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create a location with null PersistentActorReferencesAdded (should remain null)
        var location = mod.Locations.AddNew("TestLocation");
        // Don't set PersistentActorReferencesAdded - leave it null

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

        // Read the deserialized mod and verify null references are preserved
        var deserializedMod = SkyrimMod.CreateFromBinary(deserializedModPath, SkyrimRelease.SkyrimSE, new BinaryReadParameters()
        {
            FileSystem = fileSystem
        });

        var deserializedLocation = deserializedMod.Locations.First();

        // Should remain null or empty
        (deserializedLocation.PersistentActorReferencesAdded == null ||
         deserializedLocation.PersistentActorReferencesAdded.Count == 0).ShouldBeTrue();
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimLocation_SingleReference_NoSortingNeededThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create a location with single reference (no sorting needed)
        var location = mod.Locations.AddNew("TestLocation");
        location.PersistentActorReferencesAdded = new Noggog.ExtendedList<Mutagen.Bethesda.Skyrim.PersistentActorReference>();

        // Add single reference
        location.PersistentActorReferencesAdded.Add(new Mutagen.Bethesda.Skyrim.PersistentActorReference()
        {
            Grid = new P2Int16(5, 3),
            Actor = new FormKey(mod.ModKey, 0x123).ToLink<Mutagen.Bethesda.Skyrim.IPlacedNpcGetter>(),
            Location = new FormKey(mod.ModKey, 0x456).ToLink<Mutagen.Bethesda.Skyrim.IComplexLocationGetter>()
        });

        // Verify we have exactly one reference
        location.PersistentActorReferencesAdded.Count.ShouldBe(1);

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

        // Read the deserialized mod and verify single reference is preserved
        var deserializedMod = SkyrimMod.CreateFromBinary(deserializedModPath, SkyrimRelease.SkyrimSE, new BinaryReadParameters()
        {
            FileSystem = fileSystem
        });

        var deserializedLocation = deserializedMod.Locations.First();

        // Should still have exactly one reference
        deserializedLocation.PersistentActorReferencesAdded.ShouldNotBeNull();
        deserializedLocation.PersistentActorReferencesAdded.Count.ShouldBe(1);

        // Verify the reference data is preserved
        var reference = deserializedLocation.PersistentActorReferencesAdded.First();
        reference.Grid.X.ShouldBe((short)5);
        reference.Grid.Y.ShouldBe((short)3);
    }
}