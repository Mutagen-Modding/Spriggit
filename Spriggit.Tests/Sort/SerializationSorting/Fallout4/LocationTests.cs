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
        DirectoryPath tempDir,
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

        var deserializedLocation = deserializedMod.Locations.First();
        var sortedGrids = deserializedLocation.PersistentActorReferencesAdded?.Select(r => $"{r.Grid.X},{r.Grid.Y}").ToArray();

        // References should now be sorted by Grid.X, then Grid.Y
        sortedGrids.ShouldBe(new[] { "1,2", "2,1" });
    }
}