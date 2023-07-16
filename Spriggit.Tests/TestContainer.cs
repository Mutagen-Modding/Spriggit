using System.IO.Abstractions;
using Autofac;
using Noggog.Autofac;
using Noggog.IO;
using Noggog.WorkEngine;
using Serilog;
using Serilog.Core;
using Spriggit.Engine;

namespace Spriggit.Tests;

public class TestModule : Autofac.Module
{
    private readonly IFileSystem _fileSystem;

    public TestModule(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
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
        
        builder.RegisterType<FileSystem>().As<IFileSystem>()
            .SingleInstance();

        builder.RegisterInstance(Logger.None).As<ILogger>();
    }
}

public class TestContainer
{
    
}