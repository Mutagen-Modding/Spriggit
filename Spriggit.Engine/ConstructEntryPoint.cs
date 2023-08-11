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
        using var rootDir = TempFolder.FactoryByAddedPath(
            Path.Combine("Spriggit", "Sources", ident.ToString()), 
            deleteAfter: false, 
            fileSystem: _fileSystem);
        
        if (_debugState.ClearNugetSources || ident.Version.OriginalVersion.EndsWith("-zdev"))
        {
            _logger.Information("In debug mode.  Deleting entire folder {Path}", rootDir.Dir);
            _fileSystem.Directory.DeleteEntireFolder(rootDir.Dir, deleteFolderItself: false);
        }

        try
        {
            _logger.Information("Restoring for {Identifier} at {Path}", ident, rootDir.Dir);
            await _nugetDownloader.RestoreFor(ident, rootDir.Dir, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        _logger.Information("Getting target framework directory");
        var frameworkDir = _frameworkDirLocator.GetTargetFrameworkDir(Path.Combine(rootDir.Dir, $"{ident}"));
        if (frameworkDir == null) return null;
        _logger.Information("Framework directory located: {Path}", frameworkDir);
        
        _logger.Information("Publishing {Root} into framework directory {Path} for {Identifier}", rootDir.Dir, frameworkDir, ident);
        _pluginPublisher.Publish(rootDir.Dir, ident.ToString(), frameworkDir.Value);

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
}