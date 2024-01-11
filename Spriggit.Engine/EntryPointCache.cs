using Noggog;
using NuGet.Packaging.Core;
using Serilog;
using Spriggit.Core;

namespace Spriggit.Engine;

public class EntryPointCache
{
    private readonly object _lock = new();
    private readonly Dictionary<SpriggitMeta, Task<PackageIdentity?>> _metaToPackageIdentity = new();
    private readonly Dictionary<PackageIdentity, Task<EngineEntryPoint?>> _packageIdentityToEntryPt = new();
    private readonly ConstructEntryPoint _constructEntryPoint;
    private readonly NugetDownloader _nugetDownloader;
    private readonly ILogger _logger;

    public EntryPointCache(
        ConstructEntryPoint constructEntryPoint,
        NugetDownloader nugetDownloader,
        ILogger logger)
    {
        _constructEntryPoint = constructEntryPoint;
        _nugetDownloader = nugetDownloader;
        _logger = logger;
    }

    public async Task<EngineEntryPoint?> GetFor(SpriggitMeta meta, CancellationToken cancel)
    {
        Task<PackageIdentity?> identTask;
        lock (_lock)
        {
            if (!_metaToPackageIdentity.TryGetValue(meta, out identTask!))
            {
                identTask = Task.Run(async () =>
                {
                    _logger.Information("Getting first identity for {Meta}", meta);
                    var ret = await _nugetDownloader.GetFirstIdentityFor(meta, CancellationToken.None);
                    if (ret == null)
                    {
                        _logger.Warning("Could not get identity for {Meta}", meta);
                        return null;
                    }
                    _logger.Information("Cached first identity for {Meta}: {Ident}", meta, ret);
                    return ret;
                });
                _metaToPackageIdentity[meta] = identTask;
            }
        }

        var ident = await identTask.WithCancellation(cancel);
        return ident != null ? await GetFor(ident, cancel) : null;
    }

    public async Task<EngineEntryPoint?> GetFor(PackageIdentity? ident, CancellationToken cancel)
    {
        if (ident == null) return null;
        Task<EngineEntryPoint?> entryPt;
        lock (_lock)
        {
            if (!_packageIdentityToEntryPt.TryGetValue(ident, out entryPt!))
            {
                entryPt = Task.Run(async () =>
                {
                    _logger.Information("Constructing entry point for {Ident}", ident);
                    var ret = await _constructEntryPoint.ConstructFor(ident, CancellationToken.None);
                    if (ret != null)
                    {
                        _logger.Information("Cached entry point for {Ident}", ident);
                    }
                    else
                    {
                        _logger.Warning("Cached NULL entry point for {Ident}", ident);
                    }
                    return ret;
                });
                _packageIdentityToEntryPt[ident] = entryPt;
            }
        }

        return await entryPt.WithCancellation(cancel);
    }
}