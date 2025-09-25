using System.IO.Abstractions;
using Autofac;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Noggog.Testing.Extensions;
using Serilog;
using Serilog.Core;
using Shouldly;
using Spriggit.CLI.Lib.Commands.Sort;
using Spriggit.CLI.Lib.Commands.Sort.Services.Fallout4;
using Xunit;
using PerkAbilityEffect = Mutagen.Bethesda.Fallout4.PerkAbilityEffect;

namespace Spriggit.Tests.Sort;

public class SortPerksTests
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
    public async Task PerkSort(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        DirectoryPath existingDir2,
        Fallout4Mod mod,
        Mutagen.Bethesda.Fallout4.Perk perk,
        SortFallout4 sort)
    {
        perk.Effects.Add(new PerkAbilityEffect()
        {
            Priority = 5,
        });
        perk.Effects.Add(new PerkAbilityEffect()
        {
            Priority = 4,
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
}