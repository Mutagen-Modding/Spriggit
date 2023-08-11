using System.IO.Abstractions;
using McMaster.NETCore.Plugins;
using Noggog;
using Noggog.IO;
using NuGet.Packaging.Core;
using Serilog;
using Spriggit.Core;

namespace Spriggit.Engine;

public class ConstructEntryPoint
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly NugetDownloader _nugetDownloader;
    private readonly PluginPublisher _pluginPublisher;
    private readonly TargetFrameworkDirLocator _frameworkDirLocator;
    private readonly DebugState _debugState;

    public ConstructEntryPoint(
        IFileSystem fileSystem,
        ILogger logger,
        NugetDownloader nugetDownloader,
        PluginPublisher pluginPublisher,
        TargetFrameworkDirLocator frameworkDirLocator,
        DebugState debugState)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _nugetDownloader = nugetDownloader;
        _pluginPublisher = pluginPublisher;
        _frameworkDirLocator = frameworkDirLocator;
        _debugState = debugState;
    }

    public async Task<EngineEntryPoint?> ConstructFor(PackageIdentity ident, CancellationToken cancellationToken)
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

        var frameworkDir = _frameworkDirLocator.GetTargetFrameworkDir(Path.Combine(rootDir, $"{ident}"));
        if (frameworkDir == null) return null;

        var assemblyFile = Path.Combine(frameworkDir, $"{ident.Id}.dll");
        _logger.Information("Loading plugin: {Path}", assemblyFile);
        var loader = PluginLoader.CreateFromAssemblyFile(
            assemblyFile: assemblyFile,
            sharedTypes: new [] { typeof(IEntryPoint) });

        _logger.Information("Finding entry point type");
        var entryPt = loader.LoadDefaultAssembly().GetTypes()
            .FirstOrDefault(t => typeof(IEntryPoint).IsAssignableFrom(t) && !t.IsAbstract);
        if (entryPt == null)
        {
            _logger.Information("Could not find entry point");
            return null;
        }

        _logger.Information("Creating entry point object");
        var instance = Activator.CreateInstance(entryPt);
        return instance is IEntryPoint ret ? new EngineEntryPoint(ret, ident) : null;
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
            catch (InvalidOperationException)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            var frameworkDir = _frameworkDirLocator.GetTargetFrameworkDir(Path.Combine(rootDir.Dir, $"{ident}"));
            if (frameworkDir == null) return;

            _logger.Information("Publishing {Root} into framework directory {Path} for {Identifier}", rootDir.Dir, frameworkDir,
                ident);
            _pluginPublisher.Publish(rootDir.Dir, ident.ToString(), frameworkDir.Value);
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