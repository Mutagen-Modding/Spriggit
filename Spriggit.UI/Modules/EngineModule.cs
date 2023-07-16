using Autofac;
using Noggog.Autofac;
using Noggog.IO;
using Noggog.WorkEngine;
using Spriggit.Engine;

namespace Spriggit.UI.Modules;

public class EngineModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(SpriggitEngine).Assembly)
            .InNamespacesOf(
                typeof(SpriggitEngine))
            .AsImplementedInterfaces()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<NoPreferenceStreamCreator>().AsImplementedInterfaces();
        builder.RegisterType<NoPreferenceWorkDropoff>().AsImplementedInterfaces();
    }
}