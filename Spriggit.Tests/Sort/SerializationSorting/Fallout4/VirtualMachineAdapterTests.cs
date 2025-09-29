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

        var deserializedQuest = deserializedMod.Quests.First();
        var sortedPropertyNames = deserializedQuest.VirtualMachineAdapter?.Scripts[0].Properties.Select(p => p.Name).ToArray();

        // Properties should now be sorted by Name
        sortedPropertyNames.ShouldBe(new[] { "AProperty", "BProperty", "MProperty", "ZProperty" });
    }
}