using McMaster.NETCore.Plugins;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda;
using Noggog.IO;
using Noggog.WorkEngine;
using Noggog;
using Spriggit.Core;
using System.IO.Abstractions;
using System.Reflection;
using NuGet.Packaging.Core;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using Serilog;

namespace Spriggit.Engine;

public class ConstructAssemblyLoadedEntryPoint
{
    private readonly ILogger _logger;

    public ConstructAssemblyLoadedEntryPoint(ILogger logger)
    {
        _logger = logger;
    }

    public IEngineEntryPoint? GetEntryPoint(DirectoryPath frameworkDir, PackageIdentity ident)
    {
        CompositeDisposable disposable = new();
        IEntryPoint? entryPoint = null;
        Assembly? defAssembly = null;
        ISimplisticEntryPoint? simplisticEntryPoint = null;
        try
        {
            var assemblyFile = Path.Combine(frameworkDir, $"{ident.Id}.dll");
            _logger.Information("Loading plugin: {Path}", assemblyFile);
            var loader = PluginLoader.CreateFromAssemblyFile(
                assemblyFile: assemblyFile,
                sharedTypes: new[]
                {
                    typeof(IEntryPoint),
                    typeof(ISimplisticEntryPoint),
                    typeof(ICreateStream),
                    typeof(SpriggitSource),
                    typeof(SpriggitEmbeddedMeta),
                    typeof(IWorkDropoff),
                    typeof(CancellationToken),
                    typeof(GameRelease),
                    typeof(ModPath),
                    typeof(DirectoryPath),
                    typeof(IFileSystem),
                    typeof(FilePath),
                    typeof(ModKey),
                    typeof(IModKeyed),
                });
            disposable.Add(loader);

            _logger.Information("Retrieving default assembly");
            defAssembly = loader.LoadDefaultAssembly();
            entryPoint = GetEntryPointFromAssembly(defAssembly);
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Error loading IEntryPoint from assembly file");
        }

        try
        {
            if (defAssembly == null)
            {
                var assemblyFile = Path.Combine(frameworkDir, $"{ident.Id}.dll");
                _logger.Information("Loading plugin: {Path}", assemblyFile);
                var loader = PluginLoader.CreateFromAssemblyFile(
                    assemblyFile: assemblyFile,
                    sharedTypes: new[]
                    {
                        typeof(ISimplisticEntryPoint),
                        typeof(SpriggitSource),
                        typeof(SpriggitEmbeddedMeta),
                    });
                disposable.Add(loader);

                _logger.Information("Retrieving default assembly");
                defAssembly = loader.LoadDefaultAssembly();
            }
            simplisticEntryPoint = GetSimplisticEntryPointFromAssembly(defAssembly);
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Error loading ISimplisticEntryPoint from assembly file");
        }

        if (entryPoint == null && simplisticEntryPoint == null)
        {
            return null;
        }

        return new AssemblyLoadedEntryPoint(
            entryPoint,
            simplisticEntryPoint,
            ident,
            disposable);
    }

    private IEntryPoint? GetEntryPointFromAssembly(Assembly defAssembly)
    {
        _logger.Information("Trying to create entry point object");
        if (!LocateEntryPointType(defAssembly, out var entryPt)) return null;
        var assemblyEntryPointInstance = Activator.CreateInstance(entryPt);
        if (assemblyEntryPointInstance == null) return null;
        var wrappedInstance = new DynamicEntryPoint(assemblyEntryPointInstance);
        return wrappedInstance;
    }

    private ISimplisticEntryPoint? GetSimplisticEntryPointFromAssembly(Assembly defAssembly)
    {
        _logger.Information("Trying to create simplistic entry point object");
        if (!LocateSimplisticEntryPointType(defAssembly, out var entryPt)) return null;
        var assemblyEntryPointInstance = Activator.CreateInstance(entryPt);
        return assemblyEntryPointInstance as ISimplisticEntryPoint;
    }

    private bool LocateEntryPointType(Assembly assembly, [MaybeNullWhen(false)] out Type entryPt)
    {
        _logger.Information("Finding entry point type");
        _logger.Information("Looking for typically named entry point classes");
        var name = $"{assembly.FullName!.Substring(0, assembly.FullName!.IndexOf(","))}.EntryPoint";
        entryPt = assembly.GetType(name);
        if (entryPt != null) return true;
        _logger.Information("Iterating through all classes to find entry point");
        entryPt = assembly.GetTypes()
            .FirstOrDefault(t => typeof(IEntryPoint).IsAssignableFrom(t) && !t.IsAbstract);
        if (entryPt == null)
        {
            _logger.Information("Could not find entry point");
            return false;
        }

        return true;
    }

    private bool LocateSimplisticEntryPointType(Assembly assembly, [MaybeNullWhen(false)] out Type entryPt)
    {
        _logger.Information("Finding simplistic entry point type");
        _logger.Information("Looking for typically named simplistic entry point classes");
        var name = $"{assembly.FullName!.Substring(0, assembly.FullName!.IndexOf(","))}.EntryPoint";
        entryPt = assembly.GetType(name);
        if (entryPt != null) return true;
        _logger.Information("Iterating through all classes to find simplistic entry point");
        entryPt = assembly.GetTypes()
            .FirstOrDefault(t => typeof(ISimplisticEntryPoint).IsAssignableFrom(t) && !t.IsAbstract);
        if (entryPt == null)
        {
            _logger.Information("Could not find simplistic entry point");
            return false;
        }

        return true;
    }

}
