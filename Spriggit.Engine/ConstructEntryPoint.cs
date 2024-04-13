using System.IO.Abstractions;
using Noggog;
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
    private readonly FindTargetFramework _findTargetFramework;
    private readonly ConstructAssemblyLoadedEntryPoint _constructAssemblyLoadedEntryPoint;

    public ConstructEntryPoint(
        ILogger logger,
        PreparePluginFolder preparePluginFolder,
        TargetFrameworkDirLocator frameworkDirLocator,
        DebugState debugState,
        FindTargetFramework findTargetFramework,
        ConstructAssemblyLoadedEntryPoint constructAssemblyLoadedEntryPoint)
    {
        _fileSystem = IFileSystemExt.DefaultFilesystem;
        _logger = logger;
        _preparePluginFolder = preparePluginFolder;
        _frameworkDirLocator = frameworkDirLocator;
        _debugState = debugState;
        _findTargetFramework = findTargetFramework;
        _constructAssemblyLoadedEntryPoint = constructAssemblyLoadedEntryPoint;
    }

    public async Task<IEngineEntryPoint?> ConstructFor(
        DirectoryPath sourcesPath,
        PackageIdentity ident, 
        CancellationToken cancellationToken)
    {
        var rootDir = new DirectoryPath(Path.Combine(sourcesPath, ident.ToString()));
        
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

        var targetFramework = _findTargetFramework.FindTargetFrameworkWithin(packageDir);

        var frameworkDir = _frameworkDirLocator.GetTargetFrameworkDir(packageDir, targetFramework);
        if (frameworkDir == null) return null;

        return _constructAssemblyLoadedEntryPoint.GetEntryPoint(frameworkDir.Value, ident);
    }
}