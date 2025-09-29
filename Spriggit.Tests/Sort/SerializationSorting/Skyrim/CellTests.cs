using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Shouldly;
using Xunit;
using SkyrimEntryPoint = Spriggit.Yaml.Skyrim.EntryPoint;

namespace Spriggit.Tests.Sort.SerializationSorting.Skyrim;

/// <summary>
/// Tests that verify Cell reference sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Skyrim
/// </summary>
public class CellTests
{
    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimCell_PersistentReferencesSortCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath existingTempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create Cell structure: CellBlock -> CellSubBlock -> Cell
        mod.Cells.Add(new CellBlock()
        {
            BlockNumber = 1,
            SubBlocks = new ExtendedList<CellSubBlock>()
            {
                new CellSubBlock()
                {
                    BlockNumber = 2,
                    Cells = new ExtendedList<Cell>()
                    {
                        new Cell(mod.GetNextFormKey(), SkyrimRelease.SkyrimSE)
                    }
                }
            }
        });

        var cell = mod.Cells.First().SubBlocks.First().Cells.First();

        // Add persistent references in unsorted order (should sort by FormKey)
        var ref1 = new PlacedObject(mod.GetNextFormKey(), SkyrimRelease.SkyrimSE);
        var ref2 = new PlacedObject(mod.GetNextFormKey(), SkyrimRelease.SkyrimSE);
        var ref3 = new PlacedObject(mod.GetNextFormKey(), SkyrimRelease.SkyrimSE);

        // Add in reverse order to ensure they start unsorted
        cell.Persistent.Add(ref3);
        cell.Persistent.Add(ref1);
        cell.Persistent.Add(ref2);

        // Verify initial order is NOT sorted
        var initialFormKeys = cell.Persistent.Select(r => r.FormKey.ID).ToArray();
        initialFormKeys.ShouldBe(new[] { ref3.FormKey.ID, ref1.FormKey.ID, ref2.FormKey.ID });
        initialFormKeys.ShouldNotBe(initialFormKeys.OrderBy(x => x).ToArray());

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, existingTempDir, entryPoint, fileSystem);

        // Verify sorting worked correctly
        var deserializedCell = deserializedMod.Cells.First().SubBlocks.First().Cells.First();
        var sortedFormKeys = deserializedCell.Persistent.Select(r => r.FormKey.ID).ToArray();

        // References should now be sorted by FormKey ID
        deserializedCell.Persistent.Count.ShouldBe(3);
        sortedFormKeys.ShouldBe(sortedFormKeys.OrderBy(x => x).ToArray());
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimCell_TemporaryReferencesSortCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath existingTempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create Cell structure
        mod.Cells.Add(new CellBlock()
        {
            BlockNumber = 1,
            SubBlocks = new ExtendedList<CellSubBlock>()
            {
                new CellSubBlock()
                {
                    BlockNumber = 2,
                    Cells = new ExtendedList<Cell>()
                    {
                        new Cell(mod.GetNextFormKey(), SkyrimRelease.SkyrimSE)
                    }
                }
            }
        });

        var cell = mod.Cells.First().SubBlocks.First().Cells.First();

        // Add temporary references in unsorted order
        var ref1 = new PlacedObject(mod.GetNextFormKey(), SkyrimRelease.SkyrimSE);
        var ref2 = new PlacedObject(mod.GetNextFormKey(), SkyrimRelease.SkyrimSE);
        var ref3 = new PlacedObject(mod.GetNextFormKey(), SkyrimRelease.SkyrimSE);

        // Add in reverse order to ensure they start unsorted
        cell.Temporary.Add(ref3);
        cell.Temporary.Add(ref1);
        cell.Temporary.Add(ref2);

        // Verify initial order is NOT sorted
        var initialFormKeys = cell.Temporary.Select(r => r.FormKey.ID).ToArray();
        initialFormKeys.ShouldBe(new[] { ref3.FormKey.ID, ref1.FormKey.ID, ref2.FormKey.ID });
        initialFormKeys.ShouldNotBe(initialFormKeys.OrderBy(x => x).ToArray());

        // Perform serialization cycle
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, existingTempDir, entryPoint, fileSystem);

        // Verify sorting worked correctly
        var deserializedCell = deserializedMod.Cells.First().SubBlocks.First().Cells.First();
        var sortedFormKeys = deserializedCell.Temporary.Select(r => r.FormKey.ID).ToArray();

        deserializedCell.Temporary.Count.ShouldBe(3);
        sortedFormKeys.ShouldBe(sortedFormKeys.OrderBy(x => x).ToArray());
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimCell_BothPersistentAndTemporarySortCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath existingTempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create Cell structure
        mod.Cells.Add(new CellBlock()
        {
            BlockNumber = 1,
            SubBlocks = new ExtendedList<CellSubBlock>()
            {
                new CellSubBlock()
                {
                    BlockNumber = 2,
                    Cells = new ExtendedList<Cell>()
                    {
                        new Cell(mod.GetNextFormKey(), SkyrimRelease.SkyrimSE)
                    }
                }
            }
        });

        var cell = mod.Cells.First().SubBlocks.First().Cells.First();

        // Add both persistent and temporary references in unsorted order
        var pRef1 = new PlacedObject(mod.GetNextFormKey(), SkyrimRelease.SkyrimSE);
        var pRef2 = new PlacedObject(mod.GetNextFormKey(), SkyrimRelease.SkyrimSE);
        var tRef1 = new PlacedObject(mod.GetNextFormKey(), SkyrimRelease.SkyrimSE);
        var tRef2 = new PlacedObject(mod.GetNextFormKey(), SkyrimRelease.SkyrimSE);

        // Add in reverse order to ensure they start unsorted
        cell.Persistent.Add(pRef2);
        cell.Persistent.Add(pRef1);

        cell.Temporary.Add(tRef2);
        cell.Temporary.Add(tRef1);

        // Verify initial order is NOT sorted
        var initialPersistentKeys = cell.Persistent.Select(r => r.FormKey.ID).ToArray();
        var initialTemporaryKeys = cell.Temporary.Select(r => r.FormKey.ID).ToArray();
        initialPersistentKeys.ShouldNotBe(initialPersistentKeys.OrderBy(x => x).ToArray());
        initialTemporaryKeys.ShouldNotBe(initialTemporaryKeys.OrderBy(x => x).ToArray());

        // Perform serialization cycle
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, existingTempDir, entryPoint, fileSystem);

        // Verify sorting worked correctly for both lists
        var deserializedCell = deserializedMod.Cells.First().SubBlocks.First().Cells.First();
        var sortedPersistentKeys = deserializedCell.Persistent.Select(r => r.FormKey.ID).ToArray();
        var sortedTemporaryKeys = deserializedCell.Temporary.Select(r => r.FormKey.ID).ToArray();

        deserializedCell.Persistent.Count.ShouldBe(2);
        sortedPersistentKeys.ShouldBe(sortedPersistentKeys.OrderBy(x => x).ToArray());

        deserializedCell.Temporary.Count.ShouldBe(2);
        sortedTemporaryKeys.ShouldBe(sortedTemporaryKeys.OrderBy(x => x).ToArray());
    }
}