using System.IO.Abstractions;
using Autofac;
using Noggog.Autofac;
using Noggog.Processes.DI;
using Spriggit.UI.Services;
using Spriggit.UI.Settings;
using Spriggit.UI.ViewModels.Singletons;
using Spriggit.UI.ViewModels.Transient;

namespace Spriggit.UI.Modules;

public class MainModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<EngineModule>();

        builder.RegisterType<FileSystem>().As<IFileSystem>()
            .SingleInstance();
        builder.RegisterType<ProcessFactory>()
            .AsImplementedInterfaces()
            .AsSelf()
            .SingleInstance();
        
        builder.RegisterAssemblyTypes(typeof(Startup).Assembly)
            .InNamespacesOf(
                typeof(Startup),
                typeof(SettingsSingleton),
                typeof(MainVm))
            .AsImplementedInterfaces()
            .AsSelf()
            .SingleInstance();
        
        builder.RegisterAssemblyTypes(typeof(Startup).Assembly)
            .InNamespacesOf(
                typeof(LinkVm))
            .AsImplementedInterfaces()
            .AsSelf();
    }
}