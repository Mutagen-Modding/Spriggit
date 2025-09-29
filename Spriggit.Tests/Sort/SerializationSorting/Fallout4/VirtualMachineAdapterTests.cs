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
/// Tests that verify VirtualMachineAdapter (ScriptEntry) sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Fallout4
/// </summary>
public class VirtualMachineAdapterTests
{
    [Theory, MutagenModAutoData(GameRelease.Fallout4)]
    public async Task Fallout4VirtualMachineAdapter_SortsCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        Fallout4EntryPoint entryPoint,
        Fallout4Mod mod)
    {
        // Create a quest with out-of-order script properties
        var quest = mod.Quests.AddNew("TestQuest");
        quest.VirtualMachineAdapter = new Mutagen.Bethesda.Fallout4.QuestAdapter();
        quest.VirtualMachineAdapter.Scripts.Add(new Mutagen.Bethesda.Fallout4.ScriptEntry()
        {
            Name = "TestScript"
        });

        // Add properties in unsorted order
        quest.VirtualMachineAdapter.Scripts[0].Properties.Add(new Mutagen.Bethesda.Fallout4.ScriptObjectProperty() { Name = "ZProperty" });
        quest.VirtualMachineAdapter.Scripts[0].Properties.Add(new Mutagen.Bethesda.Fallout4.ScriptObjectProperty() { Name = "AProperty" });
        quest.VirtualMachineAdapter.Scripts[0].Properties.Add(new Mutagen.Bethesda.Fallout4.ScriptObjectProperty() { Name = "MProperty" });
        quest.VirtualMachineAdapter.Scripts[0].Properties.Add(new Mutagen.Bethesda.Fallout4.ScriptObjectProperty() { Name = "BProperty" });

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
}