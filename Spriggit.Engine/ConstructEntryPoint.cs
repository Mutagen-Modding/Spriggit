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
    private readonly NugetDownloader _nugetDownloader;
    private readonly PluginPublisher _pluginPublisher;
    private readonly TargetFrameworkDirLocator _frameworkDirLocator;
    private readonly DebugState _debugState;
    private readonly ConstructAssemblyLoadedEntryPoint _constructAssemblyLoadedEntryPoint;
    private readonly GetFrameworkType _getFrameworkType;

    public ConstructEntryPoint(
        IFileSystem fileSystem,
        ILogger logger,
        NugetDownloader nugetDownloader,
        PluginPublisher pluginPublisher,
        TargetFrameworkDirLocator frameworkDirLocator,
        DebugState debugState,
        ConstructAssemblyLoadedEntryPoint constructAssemblyLoadedEntryPoint,
        GetFrameworkType getFrameworkType)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _nugetDownloader = nugetDownloader;
        _pluginPublisher = pluginPublisher;
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
            await PreparePluginFolder(ident, cancellationToken, rootDir);
        }

        var packageDir = Path.Combine(rootDir, $"{ident}");
        var libDir = Path.Combine(packageDir, "lib");

        _logger.Information("Finding target framework for {Dir}", libDir);

        var targetFrameworkDir = _fileSystem.Directory
            .EnumerateDirectories(libDir)
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

    private async Task PreparePluginFolder(PackageIdentity ident, CancellationToken cancellationToken, DirectoryPath targetDir)
    {
        try
        {
            using var rootDir = TempFolder.Factory();
            try
            {
                _logger.Information("Restoring for {Identifier} at {Path}", ident, rootDir.Dir);
                await _nugetDownloader.RestoreFor(ident, rootDir.Dir, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Information("Issue restoring for {Ident} in {RootDir}",ident, rootDir.Dir);
                cancellationToken.ThrowIfCancellationRequested();
            }

            var packageDir = Path.Combine(rootDir.Dir, $"{ident}");
            var libDir = Path.Combine(packageDir, "lib");

            _logger.Information("Finding target framework for {Dir}", libDir);

            var targetFrameworkDir = _fileSystem.Directory
                .EnumerateDirectories(libDir)
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
            if (frameworkDir == null) return;

            _logger.Information("Publishing {Root} into framework directory {Path} for {Identifier}", rootDir.Dir, frameworkDir,
                ident);
            _pluginPublisher.Publish(rootDir.Dir, ident.ToString(), frameworkDir.Value, targetFramework);
            Directory.CreateDirectory(Path.GetDirectoryName(targetDir)!);
            Directory.Move(rootDir.Dir, targetDir);
        }
        catch (Exception)
        {
            targetDir.DeleteEntireFolder();
            throw;
        }
    }
}