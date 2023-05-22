using System.IO.Abstractions;
using McMaster.NETCore.Plugins;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Records;
using Noggog.IO;
using Spriggit.Core;

namespace Spriggit.Engine;

public class ConstructEntryPoint
{
    private readonly IFileSystem _fileSystem;
    private readonly NugetDownloader _nugetDownloader;
    private readonly PluginPublisher _pluginPublisher;
    private readonly TargetFrameworkDirLocator _frameworkDirLocator;

    public ConstructEntryPoint(
        IFileSystem fileSystem,
        NugetDownloader nugetDownloader,
        PluginPublisher pluginPublisher,
        TargetFrameworkDirLocator frameworkDirLocator)
    {
        _fileSystem = fileSystem;
        _nugetDownloader = nugetDownloader;
        _pluginPublisher = pluginPublisher;
        _frameworkDirLocator = frameworkDirLocator;
    }

    private EntryPointWrapper Wrap(IEntryPoint ep, GameRelease release)
    {
        return EntryPointWrapper.Wrap((dynamic)ep);
    }
    
    public async Task<IEntryPoint<IMod, IModGetter>?> ConstructFor(SpriggitMeta meta, CancellationToken cancellationToken)
    {
        var releaseSpecific = await ConstructFor($"{meta.Source.PackageName}.{meta.Release.ToCategory()}", meta.Source.Version, cancellationToken);
        if (releaseSpecific != null)
        {
            return Wrap(releaseSpecific, meta.Release);
        }

        var ret = await ConstructFor(meta.Source.PackageName, meta.Source.Version, cancellationToken) as IEntryPoint<IMod, IModGetter>;
        if (ret == null) return null;
        return Wrap(ret, meta.Release);
    }
    
    public async Task<IEntryPoint?> ConstructFor(string packageName, string? packageVersion, CancellationToken cancellationToken)
    {
        var ident = await _nugetDownloader.GetIdentityFor(packageName, packageVersion, cancellationToken);
        if (ident == null) return null;
        using var rootDir = TempFolder.FactoryByAddedPath(Path.Combine("Spriggit", "Sources", ident.ToString()), deleteAfter: false, fileSystem: _fileSystem);
        await _nugetDownloader.RestoreFor(ident, rootDir.Dir, cancellationToken);

        var frameworkDir = _frameworkDirLocator.GetTargetFrameworkDir(Path.Combine(rootDir.Dir, $"{ident}"));
        if (frameworkDir == null) return null;
        
        _pluginPublisher.Publish(rootDir.Dir, frameworkDir.Value);

        var loader = PluginLoader.CreateFromAssemblyFile(
            assemblyFile: Path.Combine(frameworkDir, $"{packageName}.dll"),
            sharedTypes: new [] { typeof(IEntryPoint<,>), typeof(IEntryPoint) });

        var entryPt = loader.LoadDefaultAssembly().GetTypes()
            .FirstOrDefault(t => typeof(IEntryPoint).IsAssignableFrom(t) && !t.IsAbstract);
        if (entryPt == null) return null;

        var ret = Activator.CreateInstance(entryPt);
        
        return ret as IEntryPoint;
    }
}