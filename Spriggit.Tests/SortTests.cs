﻿using System.IO.Abstractions;
using Autofac;
using DynamicData;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Noggog.Testing.Extensions;
using Serilog;
using Serilog.Core;
using Shouldly;
using Spriggit.CLI.Lib.Commands.Sort;
using Spriggit.CLI.Lib.Commands.Sort.Services.Fallout4;
using Spriggit.CLI.Lib.Commands.Sort.Services.Skyrim;
using Spriggit.CLI.Lib.Commands.Sort.Services.Starfield;
using Xunit;
using MorphGroup = Mutagen.Bethesda.Fallout4.MorphGroup;
using Npc = Mutagen.Bethesda.Skyrim.Npc;
using NpcFaceMorph = Mutagen.Bethesda.Starfield.NpcFaceMorph;
using PerkAbilityEffect = Mutagen.Bethesda.Fallout4.PerkAbilityEffect;
using Quest = Mutagen.Bethesda.Skyrim.Quest;
using QuestFragmentAlias = Mutagen.Bethesda.Skyrim.QuestFragmentAlias;
using ScriptBoolProperty = Mutagen.Bethesda.Skyrim.ScriptBoolProperty;
using ScriptEntry = Mutagen.Bethesda.Skyrim.ScriptEntry;
using ScriptFloatProperty = Mutagen.Bethesda.Skyrim.ScriptFloatProperty;
using ScriptProperty = Mutagen.Bethesda.Skyrim.ScriptProperty;

namespace Spriggit.Tests;

public class SortTests
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
        Npc npc,
        Quest quest,
        SortSkyrim sortSkyrim)
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
    
    [Theory, SpriggitContainerAutoData<SortTestModule>(GameRelease.Fallout4)]
    public async Task FalloutMorphGroups(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        DirectoryPath existingDir2,
        Fallout4Mod mod,
        Mutagen.Bethesda.Fallout4.Race race,
        SortFallout4 sort)
    {
        race.HeadData ??= new GenderedItem<Mutagen.Bethesda.Fallout4.HeadData?>(
            new Mutagen.Bethesda.Fallout4.HeadData()
            {
                MorphGroups = new ExtendedList<MorphGroup>()
                {
                    new MorphGroup()
                    {
                        Name = "Xyz",
                    },
                    new MorphGroup()
                    {
                        Name = "Abc",
                    }
                }
            },
            new Mutagen.Bethesda.Fallout4.HeadData()
            {
                MorphGroups = new ExtendedList<MorphGroup>()
                {
                    new MorphGroup()
                    {
                        Name = "Xyz",
                    },
                    new MorphGroup()
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
            .WriteAsync();

        var modPath2 = Path.Combine(existingDir2, mod.ModKey.FileName);

        sort.HasWorkToDo(modPath, GameRelease.Fallout4, [], null)
            .ShouldBeTrue();
        await sort.Run(modPath, GameRelease.Fallout4, modPath2, [], null);

        using var reimport = Fallout4Mod.Create(Fallout4Release.Fallout4)
            .FromPath(modPath2)
            .WithFileSystem(fileSystem)
            .Construct();
        reimport.Races.Records.Select(x => x.HeadData)
            .NotNull()
            .SelectMany(x => new []
            {
                x.Male,
                x.Female
            })
            .NotNull()
            .SelectMany(x => x.MorphGroups)
            .Select(x => x.Name)
            .ShouldEqualEnumerable("Abc", "Xyz", "Abc", "Xyz");
    }
    
    [Theory, SpriggitContainerAutoData<SortTestModule>(GameRelease.Starfield)]
    public async Task StarfieldMorphGroups(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        DirectoryPath existingDir2,
        StarfieldMod mod,
        Mutagen.Bethesda.Starfield.Npc npc,
        SortStarfield sort)
    {
        var race = mod.Races.AddNew();
        for (int key = 0; key < 64; ++key)
        {
            race.BipedObjects[(Mutagen.Bethesda.Starfield.BipedObject)key] = new();
        }
        race.ChargenAndSkintones ??= new GenderedItem<ChargenAndSkintones?>(
            new ChargenAndSkintones()
            {
                Chargen = new Chargen()
                {
                    MorphGroups = new ExtendedList<Mutagen.Bethesda.Starfield.MorphGroup>()
                    {
                        new Mutagen.Bethesda.Starfield.MorphGroup()
                        {
                            Name = "Xyz",
                        },
                        new Mutagen.Bethesda.Starfield.MorphGroup()
                        {
                            Name = "Abc",
                        }
                    }
                }
            },
            new ChargenAndSkintones()
            {
                Chargen = new Chargen()
                {
                    MorphGroups = new ExtendedList<Mutagen.Bethesda.Starfield.MorphGroup>()
                    {
                        new Mutagen.Bethesda.Starfield.MorphGroup()
                        {
                            Name = "Xyz",
                        },
                        new Mutagen.Bethesda.Starfield.MorphGroup()
                        {
                            Name = "Abc",
                        }
                    }
                }
            });

        npc.FaceMorphs.Add(new NpcFaceMorph()
        {
            MorphGroups = new ExtendedList<NpcMorphGroup>()
            {
                new NpcMorphGroup()
                {
                    MorphGroup = "Xyz"
                },
                new NpcMorphGroup()
                {
                    MorphGroup = "Abc"
                }
            }
        });
        

        var modPath = Path.Combine(existingDir, mod.ModKey.FileName);

        await mod.BeginWrite
            .ToPath(modPath)
            .WithLoadOrderFromHeaderMasters()
            .WithNoDataFolder()
            .WithFileSystem(fileSystem)
            .WriteAsync();

        var modPath2 = Path.Combine(existingDir2, mod.ModKey.FileName);

        sort.HasWorkToDo(modPath, GameRelease.Starfield, [], null)
            .ShouldBeTrue();
        await sort.Run(modPath, GameRelease.Starfield, modPath2, [], null);

        using var reimport = StarfieldMod.Create(StarfieldRelease.Starfield)
            .FromPath(modPath2)
            .WithLoadOrderFromHeaderMasters()
            .WithNoDataFolder()
            .WithFileSystem(fileSystem)
            .Construct();
        reimport.Races.Records.Select(x => x.ChargenAndSkintones)
            .NotNull()
            .SelectMany(x => new []
            {
                x.Male,
                x.Female
            })
            .NotNull()
            .SelectMany(x => x.Chargen?.MorphGroups ?? [])
            .Select(x => x.Name)
            .ShouldEqualEnumerable("Abc", "Xyz", "Abc", "Xyz");
        reimport.Npcs.Records.SelectMany(x => x.FaceMorphs)
            .SelectMany(x => x.MorphGroups)
            .Select(x => x.MorphGroup)
            .ShouldEqualEnumerable("Abc", "Xyz");
    }
    
    [Theory, SpriggitContainerAutoData<SortTestModule>(GameRelease.Starfield)]
    public async Task StarfieldMorphBlends(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        DirectoryPath existingDir2,
        StarfieldMod mod,
        Mutagen.Bethesda.Starfield.Npc npc,
        SortStarfield sort)
    {
        npc.MorphBlends.Add(new []
        {
            new NpcMorphBlend()
            {
                BlendName = "Xyz"
            },
            new NpcMorphBlend()
            {
                BlendName = "Abc"
            },
        });

        var modPath = Path.Combine(existingDir, mod.ModKey.FileName);

        await mod.BeginWrite
            .ToPath(modPath)
            .WithLoadOrderFromHeaderMasters()
            .WithNoDataFolder()
            .WithFileSystem(fileSystem)
            .WriteAsync();

        var modPath2 = Path.Combine(existingDir2, mod.ModKey.FileName);

        sort.HasWorkToDo(modPath, GameRelease.Starfield, [], null)
            .ShouldBeTrue();
        await sort.Run(modPath, GameRelease.Starfield, modPath2, [], null);

        using var reimport = StarfieldMod.Create(StarfieldRelease.Starfield)
            .FromPath(modPath2)
            .WithLoadOrderFromHeaderMasters()
            .WithNoDataFolder()
            .WithFileSystem(fileSystem)
            .Construct();
        reimport.Npcs.Records.SelectMany(x => x.MorphBlends)
            .Select(x => x.BlendName)
            .ShouldEqualEnumerable("Abc", "Xyz");
    }
    
    [Theory, SpriggitContainerAutoData<SortTestModule>(GameRelease.Fallout4)]
    public async Task PerkSort(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        DirectoryPath existingDir2,
        Fallout4Mod mod,
        Mutagen.Bethesda.Fallout4.Perk perk,
        SortFallout4 sort)
    {
        perk.Effects.Add(new []
        {
            new PerkAbilityEffect()
            {
                Priority = 5,
            },
            new PerkAbilityEffect()
            {
                Priority = 4,
            }
        });

        var modPath = Path.Combine(existingDir, mod.ModKey.FileName);

        await mod.BeginWrite
            .ToPath(modPath)
            .WithLoadOrderFromHeaderMasters()
            .WithNoDataFolder()
            .WithFileSystem(fileSystem)
            .WriteAsync();

        var modPath2 = Path.Combine(existingDir2, mod.ModKey.FileName);

        sort.HasWorkToDo(modPath, GameRelease.Fallout4, [], null)
            .ShouldBeTrue();
        await sort.Run(modPath, GameRelease.Fallout4, modPath2, [], null);

        using var reimport = Fallout4Mod.Create(Fallout4Release.Fallout4)
            .FromPath(modPath2)
            .WithFileSystem(fileSystem)
            .Construct();
        reimport.Perks.Records.SelectMany(x => x.Effects)
            .Select(x => x.Priority)
            .ShouldEqualEnumerable(4, 5);
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

    [Fact]
    public void InstantiationTest()
    {
        var service = SortCommand.GetService();
        service.ShouldNotBeNull();
    }
}