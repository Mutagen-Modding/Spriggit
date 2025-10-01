using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Shouldly;
using Spriggit.Core;
using Xunit;
using SkyrimEntryPoint = Spriggit.Yaml.Skyrim.EntryPoint;

namespace Spriggit.Tests.Sort.SerializationSorting.Skyrim;

/// <summary>
/// Tests that verify Scene sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Skyrim
/// </summary>
public class SceneCustomizationTests
{
    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimScene_SortsPhaseFragmentsByIndex(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create a scene with script fragments
        var scene = mod.Scenes.AddNew("TestScene");
        var adapter = new SceneAdapter();
        var fragments = new SceneScriptFragments();

        // Add phase fragments in unsorted order
        fragments.PhaseFragments.Add(new ScenePhaseFragment { Index = 3, ScriptName = "Phase3Script" });
        fragments.PhaseFragments.Add(new ScenePhaseFragment { Index = 1, ScriptName = "Phase1Script" });
        fragments.PhaseFragments.Add(new ScenePhaseFragment { Index = 5, ScriptName = "Phase5Script" });
        fragments.PhaseFragments.Add(new ScenePhaseFragment { Index = 2, ScriptName = "Phase2Script" });
        fragments.PhaseFragments.Add(new ScenePhaseFragment { Index = 4, ScriptName = "Phase4Script" });

        adapter.ScriptFragments = fragments;
        scene.VirtualMachineAdapter = adapter;

        // Verify initial order is NOT sorted
        var initialFragments = scene.VirtualMachineAdapter.ScriptFragments.PhaseFragments.ToArray();
        initialFragments[0].Index.ShouldBe((byte)3);
        initialFragments[1].Index.ShouldBe((byte)1);
        initialFragments[2].Index.ShouldBe((byte)5);
        initialFragments[3].Index.ShouldBe((byte)2);
        initialFragments[4].Index.ShouldBe((byte)4);

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedScene = deserializedMod.Scenes.First();
        var sortedFragments = deserializedScene.VirtualMachineAdapter?.ScriptFragments?.PhaseFragments.ToArray();

        // PhaseFragments should now be sorted by Index
        sortedFragments.ShouldNotBeNull();
        sortedFragments.Length.ShouldBe(5);
        sortedFragments[0].Index.ShouldBe((byte)1);
        sortedFragments[1].Index.ShouldBe((byte)2);
        sortedFragments[2].Index.ShouldBe((byte)3);
        sortedFragments[3].Index.ShouldBe((byte)4);
        sortedFragments[4].Index.ShouldBe((byte)5);

        // Verify script names are preserved
        sortedFragments[0].ScriptName.ShouldBe("Phase1Script");
        sortedFragments[1].ScriptName.ShouldBe("Phase2Script");
        sortedFragments[2].ScriptName.ShouldBe("Phase3Script");
        sortedFragments[3].ScriptName.ShouldBe("Phase4Script");
        sortedFragments[4].ScriptName.ShouldBe("Phase5Script");
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimScene_EmptyPhaseFragments_NoError(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create a scene without script fragments
        var scene = mod.Scenes.AddNew("TestScene");

        // Perform serialization cycle and get deserialized mod - should not throw
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedScene = deserializedMod.Scenes.First();
        deserializedScene.ShouldNotBeNull();
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE)]
    public async Task SkyrimScene_DuplicateIndices_PreservesAll(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        SkyrimEntryPoint entryPoint,
        SkyrimMod mod)
    {
        // Create a scene with duplicate indices
        var scene = mod.Scenes.AddNew("TestScene");
        var adapter = new SceneAdapter();
        var fragments = new SceneScriptFragments();

        // Add phase fragments with duplicate indices
        fragments.PhaseFragments.Add(new ScenePhaseFragment { Index = 2, ScriptName = "ScriptB" });
        fragments.PhaseFragments.Add(new ScenePhaseFragment { Index = 1, ScriptName = "ScriptA" });
        fragments.PhaseFragments.Add(new ScenePhaseFragment { Index = 2, ScriptName = "ScriptC" });

        adapter.ScriptFragments = fragments;
        scene.VirtualMachineAdapter = adapter;

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedScene = deserializedMod.Scenes.First();
        var sortedFragments = deserializedScene.VirtualMachineAdapter?.ScriptFragments?.PhaseFragments.ToArray();

        // All fragments should be preserved and sorted
        sortedFragments.ShouldNotBeNull();
        sortedFragments.Length.ShouldBe(3);
        sortedFragments[0].Index.ShouldBe((byte)1);
        sortedFragments[1].Index.ShouldBe((byte)2);
        sortedFragments[2].Index.ShouldBe((byte)2);
    }
}
