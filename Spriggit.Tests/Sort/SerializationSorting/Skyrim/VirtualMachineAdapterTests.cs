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
/// Tests that verify VirtualMachineAdapter (ScriptEntry) sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Skyrim
/// </summary>
public class VirtualMachineAdapterTests
{
    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimVirtualMachineAdapter_Quest_SortsCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create a quest with out-of-order script properties of different types
        var quest = mod.Quests.AddNew("TestQuest");
        quest.VirtualMachineAdapter = new Mutagen.Bethesda.Skyrim.QuestAdapter();
        quest.VirtualMachineAdapter.Scripts.Add(new Mutagen.Bethesda.Skyrim.ScriptEntry()
        {
            Name = "TestScript"
        });

        // Add properties in unsorted order with different types
        quest.VirtualMachineAdapter.Scripts[0].Properties.Add(new Mutagen.Bethesda.Skyrim.ScriptObjectProperty() { Name = "ZProperty" });
        quest.VirtualMachineAdapter.Scripts[0].Properties.Add(new Mutagen.Bethesda.Skyrim.ScriptBoolProperty() { Name = "AProperty" });
        quest.VirtualMachineAdapter.Scripts[0].Properties.Add(new Mutagen.Bethesda.Skyrim.ScriptFloatProperty() { Name = "MProperty" });
        quest.VirtualMachineAdapter.Scripts[0].Properties.Add(new Mutagen.Bethesda.Skyrim.ScriptObjectProperty() { Name = "BProperty" });

        // Verify initial order is NOT sorted
        var initialPropertyNames = quest.VirtualMachineAdapter.Scripts[0].Properties.Select(p => p.Name).ToArray();
        initialPropertyNames.ShouldBe(new[] { "ZProperty", "AProperty", "MProperty", "BProperty" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedQuest = deserializedMod.Quests.First();
        var sortedPropertyNames = deserializedQuest.VirtualMachineAdapter?.Scripts[0].Properties.Select(p => p.Name).ToArray();

        // Properties should now be sorted by Name
        sortedPropertyNames.ShouldBe(new[] { "AProperty", "BProperty", "MProperty", "ZProperty" });
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimVirtualMachineAdapter_Npc_SortsCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create an NPC with out-of-order script properties
        var npc = mod.Npcs.AddNew("TestNPC");
        npc.VirtualMachineAdapter = new Mutagen.Bethesda.Skyrim.VirtualMachineAdapter();
        npc.VirtualMachineAdapter.Scripts.Add(new Mutagen.Bethesda.Skyrim.ScriptEntry()
        {
            Name = "TestScript"
        });

        // Add properties in unsorted order with different types
        npc.VirtualMachineAdapter.Scripts[0].Properties.Add(new Mutagen.Bethesda.Skyrim.ScriptBoolProperty() { Name = "Xyz" });
        npc.VirtualMachineAdapter.Scripts[0].Properties.Add(new Mutagen.Bethesda.Skyrim.ScriptFloatProperty() { Name = "Abc" });

        // Verify initial order is NOT sorted
        var initialPropertyNames = npc.VirtualMachineAdapter.Scripts[0].Properties.Select(p => p.Name).ToArray();
        initialPropertyNames.ShouldBe(new[] { "Xyz", "Abc" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedNpc = deserializedMod.Npcs.First();
        var sortedPropertyNames = deserializedNpc.VirtualMachineAdapter?.Scripts[0].Properties.Select(p => p.Name).ToArray();

        // Properties should now be sorted by Name
        sortedPropertyNames.ShouldBe(new[] { "Abc", "Xyz" });
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimVirtualMachineAdapter_QuestAlias_SortsCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create a quest with alias containing out-of-order script properties
        var quest = mod.Quests.AddNew("TestQuest");
        quest.VirtualMachineAdapter = new Mutagen.Bethesda.Skyrim.QuestAdapter();
        quest.VirtualMachineAdapter.Aliases.Add(new Mutagen.Bethesda.Skyrim.QuestFragmentAlias()
        {
            Scripts = new Noggog.ExtendedList<Mutagen.Bethesda.Skyrim.ScriptEntry>()
            {
                new Mutagen.Bethesda.Skyrim.ScriptEntry()
                {
                    Name = "TestScript",
                    Properties = new Noggog.ExtendedList<Mutagen.Bethesda.Skyrim.ScriptProperty>()
                    {
                        new Mutagen.Bethesda.Skyrim.ScriptBoolProperty() { Name = "Xyz" },
                        new Mutagen.Bethesda.Skyrim.ScriptFloatProperty() { Name = "Abc" }
                    }
                }
            }
        });

        // Verify initial order is NOT sorted
        var initialPropertyNames = quest.VirtualMachineAdapter.Aliases[0].Scripts[0].Properties.Select(p => p.Name).ToArray();
        initialPropertyNames.ShouldBe(new[] { "Xyz", "Abc" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedQuest = deserializedMod.Quests.First();
        var sortedPropertyNames = deserializedQuest.VirtualMachineAdapter?.Aliases[0].Scripts[0].Properties.Select(p => p.Name).ToArray();

        // Properties should now be sorted by Name
        sortedPropertyNames.ShouldBe(new[] { "Abc", "Xyz" });
    }
}