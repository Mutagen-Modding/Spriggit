using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Reactive.Disposables;
using System.Reflection;
using McMaster.NETCore.Plugins;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Noggog;
using Noggog.IO;
using Noggog.WorkEngine;
using NuGet.Packaging.Core;
using Serilog;
using Spriggit.Core;

namespace Spriggit.Engine.Services.Singletons;

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
        try
        {
            var assemblyFile = Path.Combine(frameworkDir, $"{ident.Id}.dll");
            _logger.Information("Loading plugin: {Path}", assemblyFile);
            var loader = PluginLoader.CreateFromAssemblyFile(
                assemblyFile: assemblyFile,
                sharedTypes: new[]
                {
                    typeof(IEntryPoint),
                    typeof(ICreateStream),
                    typeof(SpriggitSource),
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

        if (entryPoint == null)
        {
            return null;
        }

        return new AssemblyLoadedEntryPoint(
            entryPoint,
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

    private bool LocateEntryPointType(Assembly assembly, [MaybeNullWhen(false)] out Type entryPt)
    {
        _logger.Information("Finding entry point type");
        _logger.Information("Looking for typically named entry point classes");
        var name = $"{assembly.FullName!.Substring(0, assembly.FullName!.IndexOf(",", StringComparison.OrdinalIgnoreCase))}.EntryPoint";
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
}
