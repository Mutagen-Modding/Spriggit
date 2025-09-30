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
/// Tests that verify QuestAdapter sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Starfield.
/// QuestAdapter inherits from VirtualMachineAdapter, so it should sort both Scripts and Fragments.
/// </summary>
public class QuestAdapterTests
{
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldQuestAdapter_SortsScriptsByName(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create a quest with out-of-order scripts
        var quest = mod.Quests.AddNew("TestQuest");
        quest.VirtualMachineAdapter = new QuestAdapter();

        // Add scripts in unsorted order
        quest.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "ZScript" });
        quest.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "AScript" });
        quest.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "MScript" });

        // Verify initial order is NOT sorted
        var initialScriptNames = quest.VirtualMachineAdapter.Scripts.Select(s => s.Name).ToArray();
        initialScriptNames.ShouldBe(new[] { "ZScript", "AScript", "MScript" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedQuest = deserializedMod.Quests.First();
        var sortedScriptNames = deserializedQuest.VirtualMachineAdapter?.Scripts.Select(s => s.Name).ToArray();

        // Scripts should now be sorted by Name (inherited from VirtualMachineAdapter)
        sortedScriptNames.ShouldBe(new[] { "AScript", "MScript", "ZScript" });
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldQuestAdapter_SortsFragmentsByStage(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create a quest with out-of-order fragments
        var quest = mod.Quests.AddNew("TestQuest");
        quest.VirtualMachineAdapter = new QuestAdapter();

        // Add fragments in unsorted order
        quest.VirtualMachineAdapter.Fragments.Add(new QuestScriptFragment() { Stage = 300 });
        quest.VirtualMachineAdapter.Fragments.Add(new QuestScriptFragment() { Stage = 10 });
        quest.VirtualMachineAdapter.Fragments.Add(new QuestScriptFragment() { Stage = 150 });
        quest.VirtualMachineAdapter.Fragments.Add(new QuestScriptFragment() { Stage = 50 });

        // Verify initial order is NOT sorted
        var initialStages = quest.VirtualMachineAdapter.Fragments.Select(f => f.Stage).ToArray();
        initialStages.ShouldBe(new ushort[] { 300, 10, 150, 50 });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedQuest = deserializedMod.Quests.First();
        var sortedStages = deserializedQuest.VirtualMachineAdapter?.Fragments.Select(f => f.Stage).ToArray();

        // Fragments should now be sorted by Stage
        sortedStages.ShouldBe(new ushort[] { 10, 50, 150, 300 });
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldQuestAdapter_SortsBothScriptsAndFragments(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create a quest with both out-of-order scripts and fragments
        var quest = mod.Quests.AddNew("TestQuest");
        quest.VirtualMachineAdapter = new QuestAdapter();

        // Add scripts in unsorted order
        quest.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "Gamma" });
        quest.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "Alpha" });
        quest.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "Beta" });

        // Add fragments in unsorted order
        quest.VirtualMachineAdapter.Fragments.Add(new QuestScriptFragment() { Stage = 200 });
        quest.VirtualMachineAdapter.Fragments.Add(new QuestScriptFragment() { Stage = 20 });
        quest.VirtualMachineAdapter.Fragments.Add(new QuestScriptFragment() { Stage = 100 });

        // Verify initial order is NOT sorted
        var initialScriptNames = quest.VirtualMachineAdapter.Scripts.Select(s => s.Name).ToArray();
        initialScriptNames.ShouldBe(new[] { "Gamma", "Alpha", "Beta" });
        var initialStages = quest.VirtualMachineAdapter.Fragments.Select(f => f.Stage).ToArray();
        initialStages.ShouldBe(new ushort[] { 200, 20, 100 });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedQuest = deserializedMod.Quests.First();

        // Both Scripts and Fragments should now be sorted
        var sortedScriptNames = deserializedQuest.VirtualMachineAdapter?.Scripts.Select(s => s.Name).ToArray();
        sortedScriptNames.ShouldBe(new[] { "Alpha", "Beta", "Gamma" });

        var sortedStages = deserializedQuest.VirtualMachineAdapter?.Fragments.Select(f => f.Stage).ToArray();
        sortedStages.ShouldBe(new ushort[] { 20, 100, 200 });
    }
}