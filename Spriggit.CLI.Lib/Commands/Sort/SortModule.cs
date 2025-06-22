using Autofac;
using Noggog.Autofac;
using Spriggit.CLI.Lib.Commands.Sort.Services;
using Spriggit.Core.Services.Singletons;

namespace Spriggit.CLI.Lib.Commands.Sort;

public class SortModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(SpriggitFileLocator).Assembly)
            .InNamespaceOf<SpriggitFileLocator>()
            .AsSelf()
            .AsMatchingInterface()
            .SingleInstance();
        builder.RegisterAssemblyTypes(typeof(SortCommandRunner).Assembly)
            .InNamespaceOf<SortCommandRunner>()
            .AsSelf()
            .AsMatchingInterface()
            .SingleInstance();
        builder
            .RegisterAssemblyTypes(typeof(SortCommandRunner).Assembly)
            .AsClosedTypesOf(typeof(ISortSomething<,>))
            .AsImplementedInterfaces()
            .SingleInstance();
    }
}