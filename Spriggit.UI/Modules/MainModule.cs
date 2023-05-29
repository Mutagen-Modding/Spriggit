using System.IO.Abstractions;
using Autofac;
using Noggog.Autofac;
using Spriggit.UI.Services;
using Spriggit.UI.Settings;
using Spriggit.UI.ViewModels;

namespace Spriggit.UI.Modules;

public class MainModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<FileSystem>().As<IFileSystem>()
            .SingleInstance();
        
        builder.RegisterAssemblyTypes(typeof(Startup).Assembly)
            .InNamespacesOf(
                typeof(Startup),
                typeof(SettingsSingleton),
                typeof(MainVm))
            .AsImplementedInterfaces()
            .AsSelf()
            .SingleInstance();
    }
}