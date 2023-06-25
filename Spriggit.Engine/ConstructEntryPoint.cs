using System.IO.Abstractions;
using McMaster.NETCore.Plugins;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;
using Noggog.IO;
using NuGet.Packaging.Core;
using Spriggit.Core;

namespace Spriggit.Engine;

public class ConstructEntryPoint
{
    private readonly IFileSystem _fileSystem;
    private readonly NugetDownloader _nugetDownloader;
    private readonly PluginPublisher _pluginPublisher;
    private readonly TargetFrameworkDirLocator _frameworkDirLocator;
    private readonly DebugState _debugState;

    public ConstructEntryPoint(
        IFileSystem fileSystem,
        NugetDownloader nugetDownloader,
        PluginPublisher pluginPublisher,
        TargetFrameworkDirLocator frameworkDirLocator,
        DebugState debugState)
    {
        _fileSystem = fileSystem;
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
        
        if (_debugState.ClearNugetSources || ident.Version.OriginalVersion.EndsWith("-dev"))
        {
            _fileSystem.Directory.DeleteEntireFolder(rootDir.Dir, deleteFolderItself: false);
        }
        
        await _nugetDownloader.RestoreFor(ident, rootDir.Dir, cancellationToken);

        var frameworkDir = _frameworkDirLocator.GetTargetFrameworkDir(Path.Combine(rootDir.Dir, $"{ident}"));
        if (frameworkDir == null) return null;
        
        _pluginPublisher.Publish(rootDir.Dir, ident.ToString(), frameworkDir.Value);

        var loader = PluginLoader.CreateFromAssemblyFile(
            assemblyFile: Path.Combine(frameworkDir, $"{ident.Id}.dll"),
            sharedTypes: new [] { typeof(IEntryPoint<,>), typeof(IEntryPoint) });

        var entryPt = loader.LoadDefaultAssembly().GetTypes()
            .FirstOrDefault(t => typeof(IEntryPoint).IsAssignableFrom(t) && !t.IsAbstract);
        if (entryPt == null) return null;

        var ret = Activator.CreateInstance(entryPt) as IEntryPoint;
        if (ret == null) return null;
        
        return new EngineEntryPoint(
            EntryPointWrapper.Wrap((dynamic)ret),
            ident);
    }
}