using System.IO.Abstractions;
using Noggog;
using Noggog.IO;
using NuGet.Packaging.Core;
using Serilog;

namespace Spriggit.Engine;

public class ConstructEntryPoint
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly PreparePluginFolder _preparePluginFolder;
    private readonly TargetFrameworkDirLocator _frameworkDirLocator;
    private readonly DebugState _debugState;
    private readonly ConstructAssemblyLoadedEntryPoint _constructAssemblyLoadedEntryPoint;
    private readonly GetFrameworkType _getFrameworkType;

    public ConstructEntryPoint(
        ILogger logger,
        PreparePluginFolder preparePluginFolder,
        TargetFrameworkDirLocator frameworkDirLocator,
        DebugState debugState,
        ConstructAssemblyLoadedEntryPoint constructAssemblyLoadedEntryPoint,
        GetFrameworkType getFrameworkType)
    {
        _fileSystem = IFileSystemExt.DefaultFilesystem;
        _logger = logger;
        _preparePluginFolder = preparePluginFolder;
        _frameworkDirLocator = frameworkDirLocator;
        _debugState = debugState;
        _constructAssemblyLoadedEntryPoint = constructAssemblyLoadedEntryPoint;
        _getFrameworkType = getFrameworkType;
    }

    public async Task<IEngineEntryPoint?> ConstructFor(PackageIdentity ident, CancellationToken cancellationToken)
    {
        var rootDir = new DirectoryPath(Path.Combine(Path.GetTempPath(), "Spriggit", "Sources", ident.ToString()));
        
        if (_debugState.ClearNugetSources && rootDir.CheckExists())
        {
            _logger.Information("In debug mode.  Deleting entire folder {Path}", rootDir);
            _fileSystem.Directory.DeleteEntireFolder(rootDir, deleteFolderItself: true);
        }

        if (!rootDir.CheckExists())
        {
            await _preparePluginFolder.Prepare(ident, cancellationToken, rootDir);
        }

        var packageDir = Path.Combine(rootDir, $"{ident}");
        var libDir = Path.Combine(packageDir, "lib");

        _logger.Information("Finding target framework for {Dir}", libDir);

        var targetFrameworkDir = _fileSystem.Directory
            .EnumerateDirectories(libDir)
            .Select(x => x)
            .OrderByDescending(x => x, new FrameworkLocatorComparer(_getFrameworkType, null))
            .FirstOrDefault();

        if (targetFrameworkDir == null)
        {
            throw new DirectoryNotFoundException($"Could not find target framework directory within {libDir}");
        }

        var targetFramework = Path.GetFileName(targetFrameworkDir);

        if (targetFramework == null)
        {
            throw new DirectoryNotFoundException($"Could not find target framework directory within {libDir}");
        }

        var frameworkDir = _frameworkDirLocator.GetTargetFrameworkDir(packageDir, targetFramework);
        if (frameworkDir == null) return null;

        return _constructAssemblyLoadedEntryPoint.GetEntryPoint(frameworkDir.Value, ident);
    }
}