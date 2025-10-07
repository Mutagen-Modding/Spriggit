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
/// Tests that verify QuestAdapter sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Skyrim.
/// QuestAdapter inherits from VirtualMachineAdapter, so it should sort both Scripts and Fragments.
/// </summary>
public class QuestAdapterTests
{
    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimQuestAdapter_SortsScriptsByName(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create a quest with out-of-order scripts
        var quest = mod.Quests.AddNew("TestQuest");
        quest.VirtualMachineAdapter = new QuestAdapter();

        // Add scripts in unsorted order
        quest.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "ZScript" });
        quest.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "AScript" });
        quest.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "MScript" });
        quest.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "BScript" });

        // Verify initial order is NOT sorted
        var initialScriptNames = quest.VirtualMachineAdapter.Scripts.Select(s => s.Name).ToArray();
        initialScriptNames.ShouldBe(new[] { "ZScript", "AScript", "MScript", "BScript" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedQuest = deserializedMod.Quests.First();
        var sortedScriptNames = deserializedQuest.VirtualMachineAdapter?.Scripts.Select(s => s.Name).ToArray();

        // Scripts should now be sorted by Name (inherited from VirtualMachineAdapter)
        sortedScriptNames.ShouldBe(new[] { "AScript", "BScript", "MScript", "ZScript" });
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimQuestAdapter_SortsFragmentsByStage(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create a quest with out-of-order fragments
        var quest = mod.Quests.AddNew("TestQuest");
        quest.VirtualMachineAdapter = new QuestAdapter();

        // Add fragments in unsorted order
        quest.VirtualMachineAdapter.Fragments.Add(new QuestScriptFragment() { Stage = 500 });
        quest.VirtualMachineAdapter.Fragments.Add(new QuestScriptFragment() { Stage = 1 });
        quest.VirtualMachineAdapter.Fragments.Add(new QuestScriptFragment() { Stage = 250 });
        quest.VirtualMachineAdapter.Fragments.Add(new QuestScriptFragment() { Stage = 100 });

        // Verify initial order is NOT sorted
        var initialStages = quest.VirtualMachineAdapter.Fragments.Select(f => f.Stage).ToArray();
        initialStages.ShouldBe(new ushort[] { 500, 1, 250, 100 });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedQuest = deserializedMod.Quests.First();
        var sortedStages = deserializedQuest.VirtualMachineAdapter?.Fragments.Select(f => f.Stage).ToArray();

        // Fragments should now be sorted by Stage
        sortedStages.ShouldBe(new ushort[] { 1, 100, 250, 500 });
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimQuestAdapter_SortsBothScriptsAndFragments(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create a quest with both out-of-order scripts and fragments
        var quest = mod.Quests.AddNew("TestQuest");
        quest.VirtualMachineAdapter = new QuestAdapter();

        // Add scripts in unsorted order
        quest.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "Zulu" });
        quest.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "Alpha" });
        quest.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "Mike" });

        // Add fragments in unsorted order
        quest.VirtualMachineAdapter.Fragments.Add(new QuestScriptFragment() { Stage = 30 });
        quest.VirtualMachineAdapter.Fragments.Add(new QuestScriptFragment() { Stage = 10 });
        quest.VirtualMachineAdapter.Fragments.Add(new QuestScriptFragment() { Stage = 20 });

        // Verify initial order is NOT sorted
        var initialScriptNames = quest.VirtualMachineAdapter.Scripts.Select(s => s.Name).ToArray();
        initialScriptNames.ShouldBe(new[] { "Zulu", "Alpha", "Mike" });
        var initialStages = quest.VirtualMachineAdapter.Fragments.Select(f => f.Stage).ToArray();
        initialStages.ShouldBe(new ushort[] { 30, 10, 20 });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedQuest = deserializedMod.Quests.First();

        // Both Scripts and Fragments should now be sorted
        var sortedScriptNames = deserializedQuest.VirtualMachineAdapter?.Scripts.Select(s => s.Name).ToArray();
        sortedScriptNames.ShouldBe(new[] { "Alpha", "Mike", "Zulu" });

        var sortedStages = deserializedQuest.VirtualMachineAdapter?.Fragments.Select(f => f.Stage).ToArray();
        sortedStages.ShouldBe(new ushort[] { 10, 20, 30 });
    }
}