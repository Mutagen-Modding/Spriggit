using System.IO.Abstractions;
using Autofac;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Starfield;
using Noggog;
using Noggog.Testing.Extensions;
using Serilog;
using Serilog.Core;
using Shouldly;
using Spriggit.CLI.Lib.Commands.Sort;
using Spriggit.CLI.Lib.Commands.Sort.Services.Fallout4;
using Spriggit.CLI.Lib.Commands.Sort.Services.Starfield;
using Xunit;
using MorphGroup = Mutagen.Bethesda.Fallout4.MorphGroup;
using ChargenAndSkintones = Mutagen.Bethesda.Starfield.ChargenAndSkintones;
using Chargen = Mutagen.Bethesda.Starfield.Chargen;
using NpcMorphGroup = Mutagen.Bethesda.Starfield.NpcMorphGroup;

namespace Spriggit.Tests.Sort.CliSorting;

public class SortMorphGroupsTests
{
    public class SortTestModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<SortModule>();
            builder.RegisterInstance(Logger.None).As<ILogger>();
        }
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
        race.HeadData = new GenderedItem<HeadData?>(
            male: new Mutagen.Bethesda.Fallout4.HeadData()
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
            female: new Mutagen.Bethesda.Fallout4.HeadData()
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

        race.ChargenAndSkintones = new GenderedItem<ChargenAndSkintones?>(
            male: new ChargenAndSkintones()
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
            female: new ChargenAndSkintones()
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

        npc.FaceMorphs.Add(new Mutagen.Bethesda.Starfield.NpcFaceMorph()
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
}