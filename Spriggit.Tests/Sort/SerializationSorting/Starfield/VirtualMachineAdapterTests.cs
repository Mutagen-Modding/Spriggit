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
/// Tests that verify VirtualMachineAdapter (ScriptEntry) sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Starfield
/// </summary>
public class VirtualMachineAdapterTests
{
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldVirtualMachineAdapter_SortsCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create a quest with out-of-order script properties
        var quest = mod.Quests.AddNew("TestQuest");
        quest.VirtualMachineAdapter = new Mutagen.Bethesda.Starfield.QuestAdapter();
        quest.VirtualMachineAdapter.Scripts.Add(new Mutagen.Bethesda.Starfield.ScriptEntry()
        {
            Name = "TestScript"
        });

        // Add properties in unsorted order
        quest.VirtualMachineAdapter.Scripts[0].Properties.Add(new Mutagen.Bethesda.Starfield.ScriptObjectProperty() { Name = "ZProperty" });
        quest.VirtualMachineAdapter.Scripts[0].Properties.Add(new Mutagen.Bethesda.Starfield.ScriptObjectProperty() { Name = "AProperty" });
        quest.VirtualMachineAdapter.Scripts[0].Properties.Add(new Mutagen.Bethesda.Starfield.ScriptObjectProperty() { Name = "MProperty" });

        // Verify initial order is NOT sorted
        var initialPropertyNames = quest.VirtualMachineAdapter.Scripts[0].Properties.Select(p => p.Name).ToArray();
        initialPropertyNames.ShouldBe(new[] { "ZProperty", "AProperty", "MProperty" });

        // Paths for serialization cycle
        var modPath = Path.Combine(tempDir.Path, mod.ModKey.FileName);
        var spriggitPath = Path.Combine(tempDir.Path, "spriggit");
        var deserializedModPath = Path.Combine(tempDir.Path, "deserialized.esm");

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
            release: GameRelease.Starfield,
            fileSystem: fileSystem,
            workDropoff: null,
            streamCreator: null,
            cancel: CancellationToken.None,
            meta: new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Starfield",
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
        var deserializedMod = StarfieldMod.CreateFromBinary(deserializedModPath, StarfieldRelease.Starfield, new BinaryReadParameters()
        {
            FileSystem = fileSystem
        });

        var deserializedQuest = deserializedMod.Quests.First();
        var sortedPropertyNames = deserializedQuest.VirtualMachineAdapter?.Scripts[0].Properties.Select(p => p.Name).ToArray();

        // Properties should now be sorted by Name
        sortedPropertyNames.ShouldBe(new[] { "AProperty", "MProperty", "ZProperty" });
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldVirtualMachineAdapter_TerminalMenu_SortsCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create a terminal menu with out-of-order script properties
        var terminalMenu = mod.TerminalMenus.AddNew("TestTerminalMenu");
        terminalMenu.VirtualMachineAdapter ??= new();
        terminalMenu.VirtualMachineAdapter.Scripts.Add(new Mutagen.Bethesda.Starfield.ScriptEntry()
        {
            Name = "TestScript"
        });

        // Add properties in unsorted order with different types
        terminalMenu.VirtualMachineAdapter.Scripts[0].Properties.Add(new Mutagen.Bethesda.Starfield.ScriptObjectListProperty() { Name = "Xyz" });
        terminalMenu.VirtualMachineAdapter.Scripts[0].Properties.Add(new Mutagen.Bethesda.Starfield.ScriptFloatProperty() { Name = "Abc" });

        // Verify initial order is NOT sorted
        var initialPropertyNames = terminalMenu.VirtualMachineAdapter.Scripts[0].Properties.Select(p => p.Name).ToArray();
        initialPropertyNames.ShouldBe(new[] { "Xyz", "Abc" });

        // Paths for serialization cycle
        var modPath = Path.Combine(tempDir.Path, mod.ModKey.FileName);
        var spriggitPath = Path.Combine(tempDir.Path, "spriggit");
        var deserializedModPath = Path.Combine(tempDir.Path, "deserialized.esm");

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
            release: GameRelease.Starfield,
            fileSystem: fileSystem,
            workDropoff: null,
            streamCreator: null,
            cancel: CancellationToken.None,
            meta: new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Starfield",
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
        var deserializedMod = StarfieldMod.CreateFromBinary(deserializedModPath, StarfieldRelease.Starfield, new BinaryReadParameters()
        {
            FileSystem = fileSystem
        });

        var deserializedTerminalMenu = deserializedMod.TerminalMenus.First();
        var sortedPropertyNames = deserializedTerminalMenu.VirtualMachineAdapter?.Scripts[0].Properties.Select(p => p.Name).ToArray();

        // Properties should now be sorted by Name
        sortedPropertyNames.ShouldBe(new[] { "Abc", "Xyz" });
    }
}