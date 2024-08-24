using System.IO.Abstractions;
using FluentAssertions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Spriggit.CLI.Lib.Commands.SortProperties;
using Xunit;

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
}