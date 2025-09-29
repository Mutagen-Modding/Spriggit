using System.IO.Abstractions;
using Autofac;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Starfield;
using Noggog;
using Noggog.Testing.Extensions;
using Serilog;
using Serilog.Core;
using Shouldly;
using Spriggit.CLI.Lib.Commands.Sort;
using Spriggit.CLI.Lib.Commands.Sort.Services.Skyrim;
using Spriggit.CLI.Lib.Commands.Sort.Services.Starfield;
using Xunit;

namespace Spriggit.Tests.Sort.CliSorting;

public class SortVirtualMachineAdapterTests
{
    public class SortTestModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<SortModule>();
            builder.RegisterInstance(Logger.None).As<ILogger>();
        }
    }

    [Theory, SpriggitContainerAutoData<SortTestModule>(GameRelease.SkyrimSE)]
    public async Task VirtualMachineAdapter(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        DirectoryPath existingDir2,
        SkyrimMod skyrimMod,
        Mutagen.Bethesda.Skyrim.Npc npc,
        Mutagen.Bethesda.Skyrim.Quest quest,
        SortSkyrim sortSkyrim)
    {
        npc.VirtualMachineAdapter ??= new();
        npc.VirtualMachineAdapter.Scripts.Add(new Mutagen.Bethesda.Skyrim.ScriptEntry()
        {
            Properties = new ExtendedList<Mutagen.Bethesda.Skyrim.ScriptProperty>()
            {
                new Mutagen.Bethesda.Skyrim.ScriptBoolProperty()
                {
                    Name = "Xyz",
                },
                new Mutagen.Bethesda.Skyrim.ScriptFloatProperty()
                {
                    Name = "Abc",
                }
            }
        });
        quest.VirtualMachineAdapter ??= new();
        quest.VirtualMachineAdapter.Aliases.Add(new Mutagen.Bethesda.Skyrim.QuestFragmentAlias()
        {
            Scripts = new ExtendedList<Mutagen.Bethesda.Skyrim.ScriptEntry>()
            {
                new Mutagen.Bethesda.Skyrim.ScriptEntry()
                {
                    Properties = new ExtendedList<Mutagen.Bethesda.Skyrim.ScriptProperty>()
                    {
                        new Mutagen.Bethesda.Skyrim.ScriptBoolProperty()
                        {
                            Name = "Xyz",
                        },
                        new Mutagen.Bethesda.Skyrim.ScriptFloatProperty()
                        {
                            Name = "Abc",
                        }
                    }
                }
            }
        });

        var modPath = Path.Combine(existingDir, skyrimMod.ModKey.FileName);

        await skyrimMod.BeginWrite
            .ToPath(modPath)
            .WithLoadOrderFromHeaderMasters()
            .WithNoDataFolder()
            .WithFileSystem(fileSystem)
            .WriteAsync();

        var modPath2 = Path.Combine(existingDir2, skyrimMod.ModKey.FileName);

        sortSkyrim.HasWorkToDo(modPath, GameRelease.SkyrimSE, [], null)
            .ShouldBeTrue();
        await sortSkyrim.Run(modPath, GameRelease.SkyrimSE, modPath2, [], null);

        using var reimport = SkyrimMod.Create(SkyrimRelease.SkyrimSE)
            .FromPath(modPath2)
            .WithFileSystem(fileSystem)
            .Construct();
        reimport.Npcs.Records.Select(x => x.VirtualMachineAdapter)
            .NotNull()
            .SelectMany(x => x.Scripts)
            .SelectMany(x => x.Properties)
            .Select(x => x.Name)
            .ShouldEqualEnumerable("Abc", "Xyz");
        reimport.Quests.Records.Select(x => x.VirtualMachineAdapter)
            .NotNull()
            .SelectMany(x => x.Aliases)
            .NotNull()
            .SelectMany(x => x.Scripts)
            .SelectMany(x => x.Properties)
            .Select(x => x.Name)
            .ShouldEqualEnumerable("Abc", "Xyz");
    }

    [Theory, SpriggitContainerAutoData<SortTestModule>(GameRelease.Starfield)]
    public async Task SpecificStarfieldTest(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        DirectoryPath existingDir2,
        StarfieldMod mod,
        TerminalMenu terminalMenu,
        string someName,
        SortStarfield sortStarfield)
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
            .ToPath(modPath)
            .WithLoadOrderFromHeaderMasters()
            .WithNoDataFolder()
            .WithFileSystem(fileSystem)
            .NoModKeySync()
            .WriteAsync();

        var modPath2 = Path.Combine(existingDir2, mod.ModKey.FileName);

        sortStarfield.HasWorkToDo(modPath, GameRelease.Starfield, [], null)
            .ShouldBeTrue();
        await sortStarfield.Run(modPath, GameRelease.Starfield, modPath2, [], null);

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
            .ShouldEqualEnumerable("Abc", "Xyz");
    }
}