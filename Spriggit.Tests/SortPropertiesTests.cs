using System.IO.Abstractions;
using FluentAssertions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Spriggit.CLI.Lib.Commands.SortProperties;
using Xunit;
using Npc = Mutagen.Bethesda.Skyrim.Npc;
using Quest = Mutagen.Bethesda.Skyrim.Quest;
using QuestFragmentAlias = Mutagen.Bethesda.Skyrim.QuestFragmentAlias;
using ScriptBoolProperty = Mutagen.Bethesda.Skyrim.ScriptBoolProperty;
using ScriptEntry = Mutagen.Bethesda.Skyrim.ScriptEntry;
using ScriptFloatProperty = Mutagen.Bethesda.Skyrim.ScriptFloatProperty;
using ScriptProperty = Mutagen.Bethesda.Skyrim.ScriptProperty;

namespace Spriggit.Tests;

public class SortPropertiesTests
{
    [Theory, MutagenModAutoData]
    public async Task Typical(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        DirectoryPath existingDir2,
        SkyrimMod skyrimMod,
        Npc npc,
        Quest quest,
        SortPropertiesSkyrim sortPropertiesSkyrim)
    {
        npc.VirtualMachineAdapter ??= new();
        npc.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
        {
            Properties = new ExtendedList<ScriptProperty>()
            {
                new ScriptBoolProperty()
                {
                    Name = "Xyz",
                },
                new ScriptFloatProperty()
                {
                    Name = "Abc",
                }
            }
        });
        quest.VirtualMachineAdapter ??= new();
        quest.VirtualMachineAdapter.Aliases.Add(new QuestFragmentAlias()
        {
            Scripts = new ExtendedList<ScriptEntry>()
            {
                new ScriptEntry()
                {
                    Properties = new ExtendedList<ScriptProperty>()
                    {
                        new ScriptBoolProperty()
                        {
                            Name = "Xyz",
                        },
                        new ScriptFloatProperty()
                        {
                            Name = "Abc",
                        }
                    }
                }
            }
        });

        var modPath = Path.Combine(existingDir, skyrimMod.ModKey.FileName);

        await skyrimMod.BeginWrite
            .WithLoadOrderFromHeaderMasters()
            .WithNoDataFolder()
            .ToPath(modPath)
            .WithFileSystem(fileSystem)
            .WriteAsync();

        var modPath2 = Path.Combine(existingDir2, skyrimMod.ModKey.FileName);

        sortPropertiesSkyrim.HasWorkToDo(modPath, GameRelease.SkyrimSE, null)
            .Should().BeTrue();
        await sortPropertiesSkyrim.Run(modPath, GameRelease.SkyrimSE, modPath2, null);

        using var reimport = SkyrimMod.Create(SkyrimRelease.SkyrimSE)
            .FromPath(modPath2)
            .WithFileSystem(fileSystem)
            .Construct();
        reimport.Npcs.Records.Select(x => x.VirtualMachineAdapter)
            .NotNull()
            .SelectMany(x => x.Scripts)
            .SelectMany(x => x.Properties)
            .Select(x => x.Name)
            .Should()
            .Equal("Abc", "Xyz");
        reimport.Quests.Records.Select(x => x.VirtualMachineAdapter)
            .NotNull()
            .SelectMany(x => x.Aliases)
            .NotNull()
            .SelectMany(x => x.Scripts)
            .SelectMany(x => x.Properties)
            .Select(x => x.Name)
            .Should()
            .Equal("Abc", "Xyz");
    }
    
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task SpecificStarfieldTest(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        DirectoryPath existingDir2,
        StarfieldMod mod,
        TerminalMenu terminalMenu,
        string someName,
        SortPropertiesStarfield sortPropertiesStarfield)
    {
        terminalMenu.VirtualMachineAdapter ??= new();
        terminalMenu.VirtualMachineAdapter.Scripts.Add(new Mutagen.Bethesda.Starfield.ScriptEntry()
        {
            Name = someName,
            Properties = new ExtendedList<Mutagen.Bethesda.Starfield.ScriptProperty>()
            {
                new Mutagen.Bethesda.Starfield.ScriptObjectListProperty()
                {
                    Name = "Xyz",
                },
                new Mutagen.Bethesda.Starfield.ScriptFloatProperty()
                {
                    Name = "Abc",
                }
            }
        });

        var modPath = Path.Combine(existingDir, mod.ModKey.FileName);

        await mod.BeginWrite
            .WithLoadOrderFromHeaderMasters()
            .WithNoDataFolder()
            .ToPath(modPath)
            .WithFileSystem(fileSystem)
            .NoModKeySync()
            .WriteAsync();

        var modPath2 = Path.Combine(existingDir2, mod.ModKey.FileName);

        sortPropertiesStarfield.HasWorkToDo(modPath, GameRelease.Starfield, null)
            .Should().BeTrue();
        await sortPropertiesStarfield.Run(modPath, GameRelease.Starfield, modPath2, null);

        using var reimport = StarfieldMod.Create(StarfieldRelease.Starfield)
            .FromPath(modPath2)
            .WithLoadOrderFromHeaderMasters()
            .WithNoDataFolder()
            .WithFileSystem(fileSystem)
            .Construct();
        reimport.TerminalMenus.Records.Select(x => x.VirtualMachineAdapter)
            .NotNull()
            .SelectMany(x => x.Scripts)
            .SelectMany(x => x.Properties)
            .Select(x => x.Name)
            .Should()
            .Equal("Abc", "Xyz");
    }
}