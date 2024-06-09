using Autofac;
using Mutagen.Bethesda.Plugins.IO.DI;
using Noggog.Autofac;
using Noggog.IO;
using Noggog.WorkEngine;
using Spriggit.Engine.Services.Singletons;

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
        builder.RegisterAssemblyTypes(typeof(IModFilesMover).Assembly)
            .InNamespacesOf(
                typeof(IModFilesMover))
            .NotInjection()
            .AsMatchingInterface();

        builder.RegisterType<NoPreferenceStreamCreator>().AsImplementedInterfaces();
        builder.RegisterType<NoPreferenceWorkDropoff>().AsImplementedInterfaces();
    }
}