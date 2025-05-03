using System.IO.Abstractions;
using Autofac;
using AutoFixture;
using AutoFixture.Xunit2;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Testing;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog.Testing.AutoFixture;

namespace Spriggit.Tests;

public class SpriggitContainerAutoData<TModule> : AutoDataAttribute
    where TModule : Module, new()
{
    public SpriggitContainerAutoData(
        GameRelease Release = GameRelease.SkyrimSE,
        TargetFileSystem FileSystem = TargetFileSystem.Fake)
        : base(() =>
        {
            var fixture = new Fixture();
            fixture.Customize(new MutagenDefaultCustomization(
                targetFileSystem: FileSystem,
                configureMembers: false,
                release: Release));
            fixture.Customize(new MutagenConcreteModsCustomization(release: Release, configureMembers: false));
            fixture.Customize(new ContainerAutoDataCustomization(
                new MutagenTestModule(
                    Release,
                    fixture.Create<IFileSystem>(),
                    [new TModule()])));
            return fixture;
        })
    {
    }
}