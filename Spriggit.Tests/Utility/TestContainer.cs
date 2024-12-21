using System.IO.Abstractions;
using Autofac;
using Mutagen.Bethesda.Plugins.IO.DI;
using Noggog.Autofac;
using Noggog.IO;
using Noggog.WorkEngine;
using Serilog;
using Serilog.Core;
using Spriggit.Core;
using Spriggit.Core.Services.Singletons;
using Spriggit.Engine.Merge;
using Spriggit.Engine.Services.Singletons;

namespace Spriggit.Tests.Utility;

public class TestModule : Autofac.Module
{
    private readonly IFileSystem _fileSystem;
    private readonly (SpriggitMeta Meta, IEntryPoint EntryPoint)[] _entryPoints;

    public TestModule(
        IFileSystem fileSystem,
        params (SpriggitMeta Meta, IEntryPoint EntryPoint)[] entryPoints)
    {
        _fileSystem = fileSystem;
        _entryPoints = entryPoints;
    }
    
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(SpriggitEngine).Assembly)
            .InNamespacesOf(
                typeof(SpriggitEngine))
            .AsImplementedInterfaces()
            .AsSelf()
            .SingleInstance();
        
        builder.RegisterAssemblyTypes(typeof(SpriggitFileLocator).Assembly)
            .InNamespacesOf(
                typeof(SpriggitFileLocator))
            .AsImplementedInterfaces()
            .AsSelf()
            .SingleInstance();
        
        builder.RegisterAssemblyTypes(typeof(FormIDCollisionFixer).Assembly)
            .InNamespacesOf(
                typeof(FormIDCollisionFixer))
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
        
        builder.RegisterInstance(_fileSystem).As<IFileSystem>()
            .SingleInstance();

        builder.RegisterInstance(Logger.None).As<ILogger>();

        if (_entryPoints.Length > 0)
        {
            builder.RegisterInstance(new ManualEntryPointCache(
                    Logger.None,
                    _entryPoints))
                .AsImplementedInterfaces();
        }
    }
}
