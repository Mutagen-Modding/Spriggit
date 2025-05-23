using System.IO.Abstractions;
using Mutagen.Bethesda.Plugins.IO.DI;
using Noggog.IO;
using Noggog.Processes.DI;
using Noggog.WorkEngine;
using Serilog;
using Spriggit.Core;
using Spriggit.Core.Services.Singletons;
using Spriggit.Engine.Services.Singletons;
using StrongInject;

namespace Spriggit.CLI.Lib;

[Register(typeof(EntryPointCache), typeof(IEntryPointCache))]
[Register(typeof(SpriggitEngine))]
[Register(typeof(GetMetaToUse))]
[Register(typeof(ConstructEntryPoint))]
[Register(typeof(ConstructCliEndpoint))]
[Register(typeof(PrepareCliFolder))]
[Register(typeof(NugetDownloader))]
[Register(typeof(PluginPublisher))]
[Register(typeof(SpriggitFileLocator))]
[Register(typeof(TargetFrameworkDirLocator))]
[Register(typeof(CurrentVersionsProvider))]
[Register(typeof(ConstructDotNetToolEndpoint))]
[Register(typeof(GetFrameworkType))]
[Register(typeof(PreparePluginFolder))]
[Register(typeof(FindTargetFramework))]
[Register(typeof(SerializeBlocker))]
[Register(typeof(PluginBackupCreator))]
[Register(typeof(SpriggitTempSourcesProvider))]
[Register(typeof(SpriggitExternalMetaPersister))]
[Register(typeof(LocalizeEnforcer))]
[Register(typeof(NugetSourceProvider))]
[Register(typeof(ProcessFactory))]
[Register(typeof(PackageVersioningChecker))]
[Register(typeof(DotNetToolTranslationPackagePathProvider))]
[Register(typeof(AssociatedFilesLocator), typeof(IAssociatedFilesLocator))]
[Register(typeof(ModFilesMover), typeof(IModFilesMover))]
[Register(typeof(ProvideCurrentTime), typeof(IProvideCurrentTime))]
[Register(typeof(PostSerializeChecker))]
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