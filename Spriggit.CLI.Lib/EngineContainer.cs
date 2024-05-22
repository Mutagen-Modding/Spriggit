using System.IO.Abstractions;
using Noggog.IO;
using Noggog.WorkEngine;
using Serilog;
using Spriggit.Engine;
using StrongInject;

namespace Spriggit.CLI;

[Register(typeof(EntryPointCache))]
[Register(typeof(SpriggitEngine))]
[Register(typeof(GetMetaToUse))]
[Register(typeof(ConstructEntryPoint))]
[Register(typeof(GetDefaultEntryPoint))]
[Register(typeof(NugetDownloader))]
[Register(typeof(PluginPublisher))]
[Register(typeof(SpriggitMetaLocator))]
[Register(typeof(TargetFrameworkDirLocator))]
[Register(typeof(CurrentVersionsProvider))]
[Register(typeof(ConstructAssemblyLoadedEntryPoint))]
[Register(typeof(GetFrameworkType))]
[Register(typeof(PreparePluginFolder))]
[Register(typeof(FindTargetFramework))]
[Register(typeof(SerializeBlocker))]
[Register(typeof(PluginBackupCreator))]
[Register(typeof(SpriggitTempSourcesProvider))]
[Register(typeof(ProvideCurrentTime), typeof(IProvideCurrentTime))]
partial class EngineContainer : IContainer<SpriggitEngine>
{
    [Instance] private readonly IFileSystem _fileSystem;
    [Instance] private readonly IWorkDropoff? _workDropoff;
    [Instance] private readonly ICreateStream? _streamCreate;
    [Instance] private readonly DebugState _debugState;
    [Instance] private readonly ILogger _logger;

    public EngineContainer(
        IFileSystem fileSystem,
        IWorkDropoff? workDropoff, 
        ICreateStream? streamCreate,
        DebugState debugState,
        ILogger logger)
    {
        _fileSystem = fileSystem;
        _workDropoff = workDropoff;
        _streamCreate = streamCreate;
        _debugState = debugState;
        _logger = logger;
    }
}