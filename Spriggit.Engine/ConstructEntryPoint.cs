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
        ConstructAssemblyLoadedEntryPoint constructAssemblyLoadedEntryPoint,
        IFileSystem fileSystem)
    {
        _logger = logger;
        _preparePluginFolder = preparePluginFolder;
        _frameworkDirLocator = frameworkDirLocator;
        _debugState = debugState;
        _findTargetFramework = findTargetFramework;
        _constructAssemblyLoadedEntryPoint = constructAssemblyLoadedEntryPoint;
        _fileSystem = fileSystem;
    }

    public Task<IEngineEntryPoint?> ConstructFor(
        DirectoryPath sourcesPath,
        PackageIdentity ident,
        CancellationToken cancellationToken)
    {
        return ConstructFor(sourcesPath, ident, cancellationToken, true);
    }

    private async Task<IEngineEntryPoint?> ConstructFor(
        DirectoryPath sourcesPath,
        PackageIdentity ident, 
        CancellationToken cancellationToken,
        bool shouldRetry)
    {
        var rootDir = new DirectoryPath(Path.Combine(sourcesPath, ident.ToString()));
        
        if (_debugState.ClearNugetSources && rootDir.CheckExists())
        {
            _logger.Information("In debug mode.  Deleting entire folder {Path}", rootDir);
            _fileSystem.Directory.DeleteEntireFolder(rootDir, deleteFolderItself: true);
        }

        if (!rootDir.CheckExists())
        {
            try
            {
                await _preparePluginFolder.Prepare(ident, cancellationToken, rootDir);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error preparing plugin folder");
                return null;
            }
            shouldRetry = false;
        }

        IEngineEntryPoint? ret;
        try
        {
            var packageDir = Path.Combine(rootDir, $"{ident}");

            var targetFramework = _findTargetFramework.FindTargetFrameworkWithin(packageDir);

            var frameworkDir = _frameworkDirLocator.GetTargetFrameworkDir(packageDir, targetFramework);
            if (frameworkDir != null)
            {
                ret = _constructAssemblyLoadedEntryPoint.GetEntryPoint(frameworkDir.Value, ident);
            }
            else
            {
                ret = null;
            }
        }
        catch (Exception e)
        when (shouldRetry)
        {
            ret = null;
            _logger.Warning("Error when constructing entry point.  Retrying: {Exception}", e);
        }

        if (ret == null && shouldRetry)
        {
            _logger.Information("Error constructing entry point.  Deleting entire folder and then retrying {Path}", rootDir);
            _fileSystem.Directory.DeleteEntireFolder(rootDir, deleteFolderItself: true);
            return await ConstructFor(sourcesPath, ident, cancellationToken, false);
        }

        return ret;
    }
}