using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Shouldly;
using Xunit;
using SkyrimSortLocations = Spriggit.CLI.Lib.Commands.Sort.Services.Skyrim.SortLocations;
using Fallout4SortLocations = Spriggit.CLI.Lib.Commands.Sort.Services.Fallout4.SortLocations;

namespace Spriggit.Tests.Sort.CliSorting;

public class SortLocationsTests
{
    [Fact]
    public void SkyrimSortLocations_NoReferencesCollection_HasNoWorkToDo()
    {
        var mod = new SkyrimMod(new ModKey("Test", ModType.Plugin), SkyrimRelease.SkyrimSE);
        var location = mod.Locations.AddNew();
        var sorter = new SkyrimSortLocations();

        sorter.HasWorkToDo(mod).ShouldBeFalse();
    }

    [Fact]
    public void SkyrimSortLocations_HasWorkToDo_DetectsUnsortedReferences()
    {
        var mod = new SkyrimMod(new ModKey("Test", ModType.Plugin), SkyrimRelease.SkyrimSE);
        var location = mod.Locations.AddNew();
        var sorter = new SkyrimSortLocations();

        if (location.PersistentActorReferencesAdded == null)
        {
            location.PersistentActorReferencesAdded = new Noggog.ExtendedList<Mutagen.Bethesda.Skyrim.PersistentActorReference>();
        }

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

        sorter.HasWorkToDo(mod).ShouldBeTrue();
    }

    [Fact]
    public void SkyrimSortLocations_DoWork_SortsReferencesCorrectly()
    {
        var mod = new SkyrimMod(new ModKey("Test", ModType.Plugin), SkyrimRelease.SkyrimSE);
        var location = mod.Locations.AddNew();
        var sorter = new SkyrimSortLocations();

        if (location.PersistentActorReferencesAdded == null)
        {
            location.PersistentActorReferencesAdded = new Noggog.ExtendedList<Mutagen.Bethesda.Skyrim.PersistentActorReference>();
        }

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

        sorter.DoWork(mod);

        var sorted = location.PersistentActorReferencesAdded.ToArray();
        sorted.Length.ShouldBe(2);

        sorted[0].Grid.X.ShouldBe((short)1);
        sorted[0].Grid.Y.ShouldBe((short)1);
        sorted[1].Grid.X.ShouldBe((short)5);
        sorted[1].Grid.Y.ShouldBe((short)3);
    }

    [Fact]
    public void Fallout4SortLocations_NoReferencesCollection_HasNoWorkToDo()
    {
        var mod = new Fallout4Mod(new ModKey("Test", ModType.Plugin), Fallout4Release.Fallout4);
        var location = mod.Locations.AddNew();
        var sorter = new Fallout4SortLocations();

        sorter.HasWorkToDo(mod).ShouldBeFalse();
    }

    [Fact]
    public void Fallout4SortLocations_HasWorkToDo_DetectsUnsortedReferences()
    {
        var mod = new Fallout4Mod(new ModKey("Test", ModType.Plugin), Fallout4Release.Fallout4);
        var location = mod.Locations.AddNew();
        var sorter = new Fallout4SortLocations();

        if (location.PersistentActorReferencesAdded == null)
        {
            location.PersistentActorReferencesAdded = new Noggog.ExtendedList<Mutagen.Bethesda.Fallout4.PersistentActorReference>();
        }

        location.PersistentActorReferencesAdded.Add(new Mutagen.Bethesda.Fallout4.PersistentActorReference()
        {
            Grid = new P2Int16(5, 3),
            Actor = new FormKey(mod.ModKey, 0x123).ToLink<Mutagen.Bethesda.Fallout4.IPlacedNpcGetter>(),
            Location = new FormKey(mod.ModKey, 0x456).ToLink<Mutagen.Bethesda.Fallout4.IComplexLocationGetter>()
        });

        location.PersistentActorReferencesAdded.Add(new Mutagen.Bethesda.Fallout4.PersistentActorReference()
        {
            Grid = new P2Int16(1, 1),
            Actor = new FormKey(mod.ModKey, 0x789).ToLink<Mutagen.Bethesda.Fallout4.IPlacedNpcGetter>(),
            Location = new FormKey(mod.ModKey, 0x456).ToLink<Mutagen.Bethesda.Fallout4.IComplexLocationGetter>()
        });

        sorter.HasWorkToDo(mod).ShouldBeTrue();
    }

    [Fact]
    public void Fallout4SortLocations_DoWork_SortsReferencesCorrectly()
    {
        var mod = new Fallout4Mod(new ModKey("Test", ModType.Plugin), Fallout4Release.Fallout4);
        var location = mod.Locations.AddNew();
        var sorter = new Fallout4SortLocations();

        if (location.PersistentActorReferencesAdded == null)
        {
            location.PersistentActorReferencesAdded = new Noggog.ExtendedList<Mutagen.Bethesda.Fallout4.PersistentActorReference>();
        }

        location.PersistentActorReferencesAdded.Add(new Mutagen.Bethesda.Fallout4.PersistentActorReference()
        {
            Grid = new P2Int16(5, 3),
            Actor = new FormKey(mod.ModKey, 0x123).ToLink<Mutagen.Bethesda.Fallout4.IPlacedNpcGetter>(),
            Location = new FormKey(mod.ModKey, 0x456).ToLink<Mutagen.Bethesda.Fallout4.IComplexLocationGetter>()
        });

        location.PersistentActorReferencesAdded.Add(new Mutagen.Bethesda.Fallout4.PersistentActorReference()
        {
            Grid = new P2Int16(1, 1),
            Actor = new FormKey(mod.ModKey, 0x789).ToLink<Mutagen.Bethesda.Fallout4.IPlacedNpcGetter>(),
            Location = new FormKey(mod.ModKey, 0x456).ToLink<Mutagen.Bethesda.Fallout4.IComplexLocationGetter>()
        });

        sorter.DoWork(mod);

        var sorted = location.PersistentActorReferencesAdded.ToArray();
        sorted.Length.ShouldBe(2);

        sorted[0].Grid.X.ShouldBe((short)1);
        sorted[0].Grid.Y.ShouldBe((short)1);
        sorted[1].Grid.X.ShouldBe((short)5);
        sorted[1].Grid.Y.ShouldBe((short)3);
    }
}