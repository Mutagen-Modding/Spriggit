using Autofac;
using Mutagen.Bethesda.Serialization.Streams;
using Noggog.Autofac;
using Noggog.WorkEngine;
using Spriggit.Engine;

namespace Spriggit.UI.Modules;

public class EngineModule: Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(SpriggitEngine).Assembly)
            .InNamespacesOf(
                typeof(SpriggitEngine))
            .Except<EntryPointWrapper>()
            .AsImplementedInterfaces()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<NoPreferenceStreamCreator>().AsImplementedInterfaces();
        builder.RegisterType<NoPreferenceWorkDropoff>().AsImplementedInterfaces();
    }
}