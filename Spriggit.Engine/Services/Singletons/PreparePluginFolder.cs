using System.IO.Abstractions;
using Noggog;
using Noggog.IO;
using NuGet.Packaging.Core;
using Serilog;

namespace Spriggit.Engine.Services.Singletons;

public class PreparePluginFolder
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly NugetDownloader _nugetDownloader;
    private readonly TargetFrameworkDirLocator _frameworkDirLocator;
    private readonly GetFrameworkType _getFrameworkType;
    private readonly PluginPublisher _pluginPublisher;

    public PreparePluginFolder(
        ILogger logger,
        NugetDownloader nugetDownloader, 
        TargetFrameworkDirLocator frameworkDirLocator,
        GetFrameworkType getFrameworkType,
        PluginPublisher pluginPublisher)
    {
        _logger = logger;
        _fileSystem = IFileSystemExt.DefaultFilesystem;
        _nugetDownloader = nugetDownloader;
        _frameworkDirLocator = frameworkDirLocator;
        _getFrameworkType = getFrameworkType;
        _pluginPublisher = pluginPublisher;
    }
    
    public async Task Prepare(PackageIdentity ident, CancellationToken cancellationToken, DirectoryPath targetDir)
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
                _logger.Error(ex, "Issue restoring for {Ident} in {RootDir}",ident, rootDir.Dir);
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