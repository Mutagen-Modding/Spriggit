using System.IO.Abstractions;
using Noggog.IO;
using Noggog.Processes.DI;
using Noggog.WorkEngine;
using Serilog;
using Spriggit.Engine.Merge;
using Spriggit.Engine.Services.Singletons;
using StrongInject;

namespace Spriggit.CLI.Lib;

[Register(typeof(FormIDReassigner))]
[Register(typeof(FormIDCollisionDetector))]
[Register(typeof(FormIDCollisionFixer))]
[Register(typeof(GetMetaToUse))]
[Register(typeof(EntryPointCache), typeof(IEntryPointCache))]
[Register(typeof(ConstructEntryPoint))]
[Register(typeof(NugetDownloader))]
[Register(typeof(ConstructAssemblyLoadedEntryPoint))]
[Register(typeof(FindTargetFramework))]
[Register(typeof(TargetFrameworkDirLocator))]
[Register(typeof(GetFrameworkType))]
[Register(typeof(NugetSourceProvider))]
[Register(typeof(SpriggitTempSourcesProvider))]
[Register(typeof(PreparePluginFolder))]
[Register(typeof(PluginPublisher))]
[Register(typeof(GitFolderLocator))]
[Register(typeof(ProcessFactory))]
[Register(typeof(ConstructCliEndpoint))]
[Register(typeof(PrepareCliFolder))]
[Register(typeof(SpriggitExternalMetaPersister))]
partial class FormIDCollisionContainer : IContainer<FormIDCollisionFixer>
{
    [Instance] private readonly IFileSystem _fileSystem;
    [Instance] private readonly ILogger _logger;
    [Instance] private readonly DebugState _debugState;
    [Instance] private readonly IWorkDropoff? _workDropoff;
    [Instance] private readonly ICreateStream? _streamCreate;

    public FormIDCollisionContainer(
        IFileSystem fileSystem,
        DebugState debugState,
        ILogger logger)
    {
        _fileSystem = fileSystem;
        _workDropoff = null;
        _streamCreate = null;
        _debugState = debugState;
        _logger = logger;
    }
}