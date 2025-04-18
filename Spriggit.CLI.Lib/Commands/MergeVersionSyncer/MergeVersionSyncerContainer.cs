using System.IO.Abstractions;
using Noggog.IO;
using Noggog.Processes.DI;
using Noggog.WorkEngine;
using Serilog;
using Spriggit.Core;
using Spriggit.Core.Services.Singletons;
using Spriggit.Engine.Merge;
using Spriggit.Engine.Services.Singletons;
using StrongInject;

namespace Spriggit.CLI.Lib.Commands.MergeVersionSyncer;

[Register(typeof(GetMetaToUse))]
[Register(typeof(EntryPointCache), typeof(IEntryPointCache))]
[Register(typeof(ConstructEntryPoint))]
[Register(typeof(NugetDownloader))]
[Register(typeof(ConstructDotNetToolEndpoint))]
[Register(typeof(FindTargetFramework))]
[Register(typeof(TargetFrameworkDirLocator))]
[Register(typeof(GetFrameworkType))]
[Register(typeof(SpriggitTempSourcesProvider))]
[Register(typeof(PreparePluginFolder))]
[Register(typeof(PluginPublisher))]
[Register(typeof(NugetSourceProvider))]
[Register(typeof(GitFolderLocator))]
[Register(typeof(ProcessFactory))]
[Register(typeof(PackageVersioningChecker))]
[Register(typeof(SpriggitExternalMetaPersister))]
[Register(typeof(ConstructCliEndpoint))]
[Register(typeof(PrepareCliFolder))]
[Register(typeof(SpriggitFileLocator))]
[Register(typeof(DotNetToolTranslationPackagePathProvider))]
[Register(typeof(Engine.Merge.MergeVersionSyncer))]
partial class MergeVersionSyncerContainer : IContainer<Engine.Merge.MergeVersionSyncer>
{
    [Instance] private readonly IFileSystem _fileSystem;
    [Instance] private readonly ILogger _logger;
    [Instance] private readonly DebugState _debugState;
    [Instance] private readonly IWorkDropoff? _workDropoff;
    [Instance] private readonly ICreateStream? _streamCreate;

    public MergeVersionSyncerContainer(
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