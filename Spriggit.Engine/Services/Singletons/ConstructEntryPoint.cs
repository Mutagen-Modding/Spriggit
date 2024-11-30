using System.IO.Abstractions;
using Noggog;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Serilog;

namespace Spriggit.Engine.Services.Singletons;

public class ConstructEntryPoint
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly PreparePluginFolder _preparePluginFolder;
    private readonly TargetFrameworkDirLocator _frameworkDirLocator;
    private readonly DebugState _debugState;
    private readonly ConstructCliEndpoint _constructCliEndpoint;
    private readonly FindTargetFramework _findTargetFramework;
    private readonly ConstructAssemblyLoadedEntryPoint _constructAssemblyLoadedEntryPoint;
    private readonly ConstructDotNetToolEndpoint _constructDotNetToolEntryPoint;
    private readonly NuGetVersion _legacyVersion = new(0, 35, 1, 0);

    public ConstructEntryPoint(
        ILogger logger,
        IFileSystem fileSystem, 
        PreparePluginFolder preparePluginFolder,
        TargetFrameworkDirLocator frameworkDirLocator,
        DebugState debugState,
        ConstructCliEndpoint constructCliEndpoint,
        FindTargetFramework findTargetFramework,
        ConstructAssemblyLoadedEntryPoint constructAssemblyLoadedEntryPoint,
        ConstructDotNetToolEndpoint constructDotNetToolEntryPoint)
    {
        _logger = logger;
        _preparePluginFolder = preparePluginFolder;
        _frameworkDirLocator = frameworkDirLocator;
        _debugState = debugState;
        _constructCliEndpoint = constructCliEndpoint;
        _findTargetFramework = findTargetFramework;
        _constructAssemblyLoadedEntryPoint = constructAssemblyLoadedEntryPoint;
        _fileSystem = fileSystem;
        _constructDotNetToolEntryPoint = constructDotNetToolEntryPoint;
    }

    public async Task<IEngineEntryPoint?> ConstructFor(
        DirectoryPath tempPath,
        PackageIdentity ident,
        CancellationToken cancellationToken)
    {
        // ToDo
        // Don't assume it's a Spriggit package.  Others might have their own versioning
        if (ident.Version <= _legacyVersion)
        {
            return await ConstructForLegacy(tempPath, ident, cancellationToken, shouldRetry: false);
        }
        
        return await _constructDotNetToolEntryPoint.ConstructFor(tempPath, ident, cancellationToken);
    }

    private async Task<IEngineEntryPoint?> ConstructForLegacy(
        DirectoryPath tempPath,
        PackageIdentity ident, 
        CancellationToken cancellationToken,
        bool shouldRetry)
    {
        var sourcesPath = Path.Combine(tempPath, SpriggitTempSourcesProvider.SourcesSubPath);
        var packageUnpackFolder = new DirectoryPath(Path.Combine(sourcesPath, ident.ToString()));

        if (_debugState.ClearNugetSources && packageUnpackFolder.CheckExists())
        {
            _logger.Information("In debug mode.  Deleting entire folder {Path}", packageUnpackFolder);
            _fileSystem.Directory.DeleteEntireFolder(packageUnpackFolder, deleteFolderItself: true);
        }

        if (!packageUnpackFolder.CheckExists())
        {
            try
            {
                await _preparePluginFolder.Prepare(ident, cancellationToken, packageUnpackFolder);
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
            var packageDir = Path.Combine(packageUnpackFolder, $"{ident}");

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
            _logger.Information("Error constructing entry point.  Deleting entire folder and then retrying {Path}", packageUnpackFolder);
            _fileSystem.Directory.DeleteEntireFolder(packageUnpackFolder, deleteFolderItself: true);
            return await ConstructForLegacy(sourcesPath, ident, cancellationToken, false);
        }

        var cliEndpoint = await _constructCliEndpoint.ConstructFor(tempPath, ident, cancellationToken);
        if (cliEndpoint == null) return ret;
        if (ret == null) return new EngineEntryPointWrapper(_logger, ident, 
            cliEndpoint);

        return new EngineEntryPointWrapper(
            _logger,
            ident,
            ret,
            cliEndpoint);
    }
}