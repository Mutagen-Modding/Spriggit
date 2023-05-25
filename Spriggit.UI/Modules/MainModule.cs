using System.IO.Abstractions;
using Autofac;
using Noggog.Autofac;
using Spriggit.UI.Services;

namespace Spriggit.UI.Modules;

public class MainModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<FileSystem>().As<IFileSystem>()
            .SingleInstance();
        
        builder.RegisterAssemblyTypes(typeof(Startup).Assembly)
            .InNamespacesOf(
                typeof(Startup))
            .AsImplementedInterfaces()
            .AsSelf()
            .SingleInstance();
    }
}