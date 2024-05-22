using System.IO.Abstractions;
using Mutagen.Bethesda.Plugins.Records.Mapping;
using Noggog.IO;
using Noggog.WorkEngine;
using Serilog;
using Spriggit.Engine;
using Spriggit.Engine.Collision;
using StrongInject;

namespace Spriggit.CLI;

[Register(typeof(FormIDReassigner))]
[Register(typeof(FormIDCollisionDetector))]
[Register(typeof(FormIDCollisionFixer))]
[Register(typeof(GetMetaToUse))]
[Register(typeof(EntryPointCache))]
[Register(typeof(ConstructEntryPoint))]
[Register(typeof(NugetDownloader))]
[Register(typeof(ConstructAssemblyLoadedEntryPoint))]
[Register(typeof(FindTargetFramework))]
[Register(typeof(TargetFrameworkDirLocator))]
[Register(typeof(GetDefaultEntryPoint))]
[Register(typeof(GetFrameworkType))]
[Register(typeof(SpriggitTempSourcesProvider))]
[Register(typeof(PreparePluginFolder))]
[Register(typeof(PluginPublisher))]
[Register(typeof(GitFolderLocator))]
[Register(typeof(SpriggitEmbeddedMetaPersister))]
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