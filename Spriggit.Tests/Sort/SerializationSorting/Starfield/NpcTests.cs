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
/// Tests that verify NPC sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Starfield
/// </summary>
public class NpcTests
{
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldNpcMorphBlends_SortsCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create a new NPC to avoid FormKey conflicts
        var npc = mod.Npcs.AddNew("TestNPC");

        // Add morph blends in unsorted order (should sort by BlendName)
        npc.MorphBlends.Add(new NpcMorphBlend() { BlendName = "ZBlend" });
        npc.MorphBlends.Add(new NpcMorphBlend() { BlendName = "ABlend" });
        npc.MorphBlends.Add(new NpcMorphBlend() { BlendName = "MBlend" });

        // Verify initial order is NOT sorted
        var initialBlendNames = npc.MorphBlends.Select(mb => mb.BlendName).ToArray();
        initialBlendNames.ShouldBe(new[] { "ZBlend", "ABlend", "MBlend" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedNpc = deserializedMod.Npcs.First();
        var sortedBlendNames = deserializedNpc.MorphBlends.Select(mb => mb.BlendName).ToArray();

        // Morph blends should now be sorted by BlendName
        sortedBlendNames.ShouldBe(new[] { "ABlend", "MBlend", "ZBlend" });
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldNpcFaceMorph_SortsCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create a new NPC with face morphs having out-of-order morph groups
        var npc = mod.Npcs.AddNew("TestNPC");
        npc.FaceMorphs.Add(new Mutagen.Bethesda.Starfield.NpcFaceMorph()
        {
            MorphGroups = new Noggog.ExtendedList<Mutagen.Bethesda.Starfield.NpcMorphGroup>()
            {
                new Mutagen.Bethesda.Starfield.NpcMorphGroup() { MorphGroup = "Xyz" },
                new Mutagen.Bethesda.Starfield.NpcMorphGroup() { MorphGroup = "Abc" },
                new Mutagen.Bethesda.Starfield.NpcMorphGroup() { MorphGroup = "Mno" }
            }
        });

        // Verify initial order is NOT sorted
        var initialMorphGroupNames = npc.FaceMorphs[0].MorphGroups.Select(mg => mg.MorphGroup).ToArray();
        initialMorphGroupNames.ShouldBe(new[] { "Xyz", "Abc", "Mno" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedNpc = deserializedMod.Npcs.First();
        var sortedMorphGroupNames = deserializedNpc.FaceMorphs[0].MorphGroups.Select(mg => mg.MorphGroup).ToArray();

        // Morph groups should now be sorted by MorphGroup name
        sortedMorphGroupNames.ShouldBe(new[] { "Abc", "Mno", "Xyz" });
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldNpc_ActorEffectsSortCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create a new NPC to avoid FormKey conflicts
        var npc = mod.Npcs.AddNew("TestNPC");

        // Create spell effects with FormKeys in unsorted order
        var effect1 = mod.Spells.AddNew("Effect1");
        var effect2 = mod.Spells.AddNew("Effect2");
        var effect3 = mod.Spells.AddNew("Effect3");

        // Add in reverse order to ensure they start unsorted
        npc.ActorEffect.Add(effect3);
        npc.ActorEffect.Add(effect1);
        npc.ActorEffect.Add(effect2);

        // Verify initial order is NOT sorted
        var initialEffectKeys = npc.ActorEffect.Select(e => e.FormKey.ID).ToArray();
        initialEffectKeys.ShouldBe(new[] { effect3.FormKey.ID, effect1.FormKey.ID, effect2.FormKey.ID });
        initialEffectKeys.ShouldNotBe(initialEffectKeys.OrderBy(x => x).ToArray());

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedNpc = deserializedMod.Npcs.First();
        var sortedEffectKeys = deserializedNpc.ActorEffect.Select(e => e.FormKey.ID).ToArray();

        // Effects should now be sorted by FormKey
        deserializedNpc.ActorEffect.Count.ShouldBe(3);
        sortedEffectKeys.ShouldBe(sortedEffectKeys.OrderBy(x => x).ToArray());
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldNpc_FactionsSortCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create a new NPC to avoid FormKey conflicts
        var npc = mod.Npcs.AddNew("TestNPC");

        // Create factions with FormKeys
        var faction1 = mod.Factions.AddNew("Faction1");
        var faction2 = mod.Factions.AddNew("Faction2");
        var faction3 = mod.Factions.AddNew("Faction3");

        // Add in reverse order with varying ranks
        npc.Factions.Add(new RankPlacement() { Faction = faction3.ToLink(), Rank = 1 });
        npc.Factions.Add(new RankPlacement() { Faction = faction1.ToLink(), Rank = 2 });
        npc.Factions.Add(new RankPlacement() { Faction = faction2.ToLink(), Rank = 0 });

        // Verify initial order is NOT sorted
        var initialFactionKeys = npc.Factions.Select(f => f.Faction.FormKey.ID).ToArray();
        initialFactionKeys.ShouldBe(new[] { faction3.FormKey.ID, faction1.FormKey.ID, faction2.FormKey.ID });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedNpc = deserializedMod.Npcs.First();
        var sortedFactionKeys = deserializedNpc.Factions.Select(f => f.Faction.FormKey.ID).ToArray();

        // Factions should now be sorted by FormKey, then by Rank
        deserializedNpc.Factions.Count.ShouldBe(3);
        sortedFactionKeys.ShouldBe(sortedFactionKeys.OrderBy(x => x).ToArray());
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldNpc_ItemsSortCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create a new NPC to avoid FormKey conflicts
        var npc = mod.Npcs.AddNew("TestNPC");

        // Create items with FormKeys
        var item1 = mod.MiscItems.AddNew("Item1");
        var item2 = mod.MiscItems.AddNew("Item2");
        var item3 = mod.MiscItems.AddNew("Item3");

        // Initialize Items list if null
        npc.Items ??= new();

        // Add in unsorted order
        npc.Items.Add(new ContainerEntry() { Item = new ContainerItem() { Item = item3.ToLink(), Count = 5 } });
        npc.Items.Add(new ContainerEntry() { Item = new ContainerItem() { Item = item1.ToLink(), Count = 10 } });
        npc.Items.Add(new ContainerEntry() { Item = new ContainerItem() { Item = item2.ToLink(), Count = 2 } });

        // Verify initial order is NOT sorted
        var initialItemKeys = npc.Items.Select(i => i.Item.Item.FormKey.ID).ToArray();
        initialItemKeys.ShouldBe(new[] { item3.FormKey.ID, item1.FormKey.ID, item2.FormKey.ID });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedNpc = deserializedMod.Npcs.First();
        var sortedItemKeys = deserializedNpc.Items?.Select(i => i.Item.Item.FormKey.ID).ToArray() ?? Array.Empty<uint>();

        // Items should now be sorted by FormKey
        deserializedNpc.Items!.Count.ShouldBe(3);
        sortedItemKeys.ShouldBe(sortedItemKeys.OrderBy(x => x).ToArray());
    }
}