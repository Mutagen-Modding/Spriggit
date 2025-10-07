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
/// Tests that verify VirtualMachineAdapter Scripts sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Starfield
/// </summary>
public class VirtualMachineAdapterTests
{
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldVirtualMachineAdapter_Armor_SortsScriptsByName(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create an armor with out-of-order scripts
        var armor = mod.Armors.AddNew("TestArmor");
        armor.VirtualMachineAdapter = new VirtualMachineAdapter();

        // Add scripts in unsorted order
        armor.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "ZScript" });
        armor.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "AScript" });
        armor.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "MScript" });

        // Verify initial order is NOT sorted
        var initialScriptNames = armor.VirtualMachineAdapter.Scripts.Select(s => s.Name).ToArray();
        initialScriptNames.ShouldBe(new[] { "ZScript", "AScript", "MScript" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedArmor = deserializedMod.Armors.First();
        var sortedScriptNames = deserializedArmor.VirtualMachineAdapter?.Scripts.Select(s => s.Name).ToArray();

        // Scripts should now be sorted by Name
        sortedScriptNames.ShouldBe(new[] { "AScript", "MScript", "ZScript" });
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldVirtualMachineAdapter_Weapon_SortsScriptsByName(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create a weapon with out-of-order scripts
        var weapon = mod.Weapons.AddNew("TestWeapon");
        weapon.VirtualMachineAdapter = new VirtualMachineAdapter();

        // Add scripts in unsorted order
        weapon.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "Gamma" });
        weapon.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "Alpha" });
        weapon.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "Beta" });
        weapon.VirtualMachineAdapter.Scripts.Add(new ScriptEntry() { Name = "Delta" });

        // Verify initial order is NOT sorted
        var initialScriptNames = weapon.VirtualMachineAdapter.Scripts.Select(s => s.Name).ToArray();
        initialScriptNames.ShouldBe(new[] { "Gamma", "Alpha", "Beta", "Delta" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedWeapon = deserializedMod.Weapons.First();
        var sortedScriptNames = deserializedWeapon.VirtualMachineAdapter?.Scripts.Select(s => s.Name).ToArray();

        // Scripts should now be sorted by Name
        sortedScriptNames.ShouldBe(new[] { "Alpha", "Beta", "Delta", "Gamma" });
    }
}