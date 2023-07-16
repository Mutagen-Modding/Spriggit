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
[Register(typeof(TargetFrameworkDirLocator))]
partial class Container : IContainer<SpriggitEngine>
{
    [Instance] private readonly IFileSystem _fileSystem;
    [Instance] private readonly IWorkDropoff? _workDropoff;
    [Instance] private readonly ICreateStream? _streamCreate;
    [Instance] private readonly DebugState _debugState;
    [Instance] private readonly ILogger _logger;

    public Container(
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