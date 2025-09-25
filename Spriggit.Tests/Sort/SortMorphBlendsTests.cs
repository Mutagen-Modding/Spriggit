using System.IO.Abstractions;
using Autofac;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Noggog.Testing.Extensions;
using Serilog;
using Serilog.Core;
using Shouldly;
using Spriggit.CLI.Lib.Commands.Sort;
using Spriggit.CLI.Lib.Commands.Sort.Services.Starfield;
using Xunit;

namespace Spriggit.Tests.Sort;

public class SortMorphBlendsTests
{
    public class SortTestModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<SortModule>();
            builder.RegisterInstance(Logger.None).As<ILogger>();
        }
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
        npc.MorphBlends.Add(new NpcMorphBlend()
        {
            BlendName = "Xyz"
        });
        npc.MorphBlends.Add(new NpcMorphBlend()
        {
            BlendName = "Abc"
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
}