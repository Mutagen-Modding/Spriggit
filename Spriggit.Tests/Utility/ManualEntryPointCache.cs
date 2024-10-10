using Noggog;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Serilog;
using Spriggit.Core;
using Spriggit.Engine;
using Spriggit.Engine.Services.Singletons;

namespace Spriggit.Tests.Utility;

public class ManualEntryPointCache : IEntryPointCache
{
    private readonly ILogger _logger;
    private readonly Dictionary<SpriggitMeta, EngineEntryPointWrapper> _entryPoints;
    private readonly Dictionary<PackageIdentity, EngineEntryPointWrapper> _entryPointsByIdentity;
    
    public ManualEntryPointCache(
        ILogger logger,
        params (SpriggitMeta Meta, IEntryPoint EntryPoint)[] entryPoints)
    {
        _logger = logger;
        _entryPoints = entryPoints
            .ToDictionary(
                x => x.Meta,
                x => new EngineEntryPointWrapper(_logger, ToPackageIdentity(x.Meta), x.EntryPoint));
        _entryPointsByIdentity = entryPoints
            .ToDictionary(
                x => ToPackageIdentity(x.Meta),
                x => new EngineEntryPointWrapper(_logger, ToPackageIdentity(x.Meta), x.EntryPoint));
    }

    private static PackageIdentity ToPackageIdentity(SpriggitMeta x)
    {
        return new PackageIdentity(x.Source.PackageName, NuGetVersion.Parse(x.Source.Version));
    }
    
    public async Task<IEngineEntryPoint?> GetFor(SpriggitMeta meta, CancellationToken cancel)
    {
        return _entryPoints.GetOrDefault(meta);
    }

    public async Task<IEngineEntryPoint?> GetFor(PackageIdentity? ident, CancellationToken cancel)
    {
        return ident == null ? null : _entryPointsByIdentity.GetOrDefault(ident);
    }
}