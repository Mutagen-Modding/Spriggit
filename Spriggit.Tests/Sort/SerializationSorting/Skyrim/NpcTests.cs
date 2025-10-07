using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Shouldly;
using Xunit;
using SkyrimEntryPoint = Spriggit.Yaml.Skyrim.EntryPoint;

namespace Spriggit.Tests.Sort.SerializationSorting.Skyrim;

/// <summary>
/// Tests that verify NPC sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Skyrim
/// </summary>
public class NpcTests
{
    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimNpc_ActorEffectsSortCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath existingTempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create a new NPC to avoid FormKey conflicts
        var npc = mod.Npcs.AddNew("TestNPC");

        // Create spell effects with FormKeys in unsorted order
        var effect1 = mod.Spells.AddNew("Effect1");
        var effect2 = mod.Spells.AddNew("Effect2");
        var effect3 = mod.Spells.AddNew("Effect3");

        // Initialize ActorEffect list if null
        npc.ActorEffect ??= new();

        // Add in reverse order to ensure they start unsorted
        npc.ActorEffect.Add(effect3);
        npc.ActorEffect.Add(effect1);
        npc.ActorEffect.Add(effect2);

        // Verify initial order is NOT sorted
        var initialEffectKeys = npc.ActorEffect.Select(e => e.FormKey.ID).ToArray();
        initialEffectKeys.ShouldBe(new[] { effect3.FormKey.ID, effect1.FormKey.ID, effect2.FormKey.ID });
        initialEffectKeys.ShouldNotBe(initialEffectKeys.OrderBy(x => x).ToArray());

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, existingTempDir, entryPoint, fileSystem);

        var deserializedNpc = deserializedMod.Npcs.First();
        var sortedEffectKeys = deserializedNpc.ActorEffect?.Select(e => e.FormKey.ID).ToArray() ?? Array.Empty<uint>();

        // Effects should now be sorted by FormKey
        deserializedNpc.ActorEffect!.Count.ShouldBe(3);
        sortedEffectKeys.ShouldBe(sortedEffectKeys.OrderBy(x => x).ToArray());
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimNpc_FactionsSortCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath existingTempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
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
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, existingTempDir, entryPoint, fileSystem);

        var deserializedNpc = deserializedMod.Npcs.First();
        var sortedFactionKeys = deserializedNpc.Factions.Select(f => f.Faction.FormKey.ID).ToArray();

        // Factions should now be sorted by FormKey, then by Rank
        deserializedNpc.Factions.Count.ShouldBe(3);
        sortedFactionKeys.ShouldBe(sortedFactionKeys.OrderBy(x => x).ToArray());
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimNpc_ItemsSortCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath existingTempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
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
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, existingTempDir, entryPoint, fileSystem);

        var deserializedNpc = deserializedMod.Npcs.First();
        var sortedItemKeys = deserializedNpc.Items?.Select(i => i.Item.Item.FormKey.ID).ToArray() ?? Array.Empty<uint>();

        // Items should now be sorted by FormKey
        deserializedNpc.Items!.Count.ShouldBe(3);
        sortedItemKeys.ShouldBe(sortedItemKeys.OrderBy(x => x).ToArray());
    }

    // TODO: Re-enable when AttackData property name is determined
    // [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    // public async Task SkyrimNpc_AttacksSortCorrectlyThroughSerialization(
    //     IFileSystem fileSystem,
    //     DirectoryPath existingTempDir,
    //     SkyrimEntryPoint entryPoint,
    //     SkyrimMod mod)
    // {
    //     // Create a new NPC to avoid FormKey conflicts
    //     var npc = mod.Npcs.AddNew("TestNPC");
    //
    //     // Add attacks in unsorted order (by AttackEvent string)
    //     npc.Attacks.Add(new AttackData() { AttackEvent = "ZAttack" });
    //     npc.Attacks.Add(new AttackData() { AttackEvent = "AAttack" });
    //     npc.Attacks.Add(new AttackData() { AttackEvent = "MAttack" });
    //
    //     // Verify initial order is NOT sorted
    //     var initialAttackEvents = npc.Attacks.Select(a => a.AttackEvent).ToArray();
    //     initialAttackEvents.ShouldBe(new[] { "ZAttack", "AAttack", "MAttack" });
    //
    //     // Perform serialization cycle and get deserialized mod
    //     var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, existingTempDir, entryPoint, fileSystem);
    //
    //     var deserializedNpc = deserializedMod.Npcs.First();
    //     var sortedAttackEvents = deserializedNpc.Attacks?.Select(a => a.AttackEvent).ToArray() ?? Array.Empty<string?>();
    //
    //     // Attacks should now be sorted by AttackEvent
    //     deserializedNpc.Attacks!.Count.ShouldBe(3);
    //     sortedAttackEvents.ShouldBe(new[] { "AAttack", "MAttack", "ZAttack" });
    // }
}