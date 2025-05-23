﻿using System.IO.Abstractions;
using Mutagen.Bethesda.Plugins.IO.DI;
using Noggog.IO;
using Noggog.Processes.DI;
using Noggog.WorkEngine;
using Serilog;
using Spriggit.Core;
using Spriggit.Core.Services.Singletons;
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
[Register(typeof(ConstructDotNetToolEndpoint))]
[Register(typeof(FindTargetFramework))]
[Register(typeof(TargetFrameworkDirLocator))]
[Register(typeof(GetFrameworkType))]
[Register(typeof(NugetSourceProvider))]
[Register(typeof(SpriggitTempSourcesProvider))]
[Register(typeof(PreparePluginFolder))]
[Register(typeof(PluginPublisher))]
[Register(typeof(GitFolderLocator))]
[Register(typeof(PackageVersioningChecker))]
[Register(typeof(ProcessFactory))]
[Register(typeof(ConstructCliEndpoint))]
[Register(typeof(PrepareCliFolder))]
[Register(typeof(SpriggitFileLocator))]
[Register(typeof(SpriggitExternalMetaPersister))]
[Register(typeof(SpriggitEngine))]
[Register(typeof(ModFilesMover), typeof(IModFilesMover))]
[Register(typeof(LocalizeEnforcer))]
[Register(typeof(PostSerializeChecker))]
[Register(typeof(SerializeBlocker))]
[Register(typeof(PluginBackupCreator))]
[Register(typeof(DotNetToolTranslationPackagePathProvider))]
[Register(typeof(ProvideCurrentTime), typeof(IProvideCurrentTime))]
[Register(typeof(AssociatedFilesLocator), typeof(IAssociatedFilesLocator))]
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