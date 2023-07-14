using Mutagen.Bethesda.Plugins.Records;
using NuGet.Packaging.Core;
using Spriggit.Core;

namespace Spriggit.Engine;

public class EntryPointCache
{
    private readonly object _lock = new();
    private readonly Dictionary<SpriggitMeta, Task<PackageIdentity?>> _metaToPackageIdentity = new();
    private readonly Dictionary<PackageIdentity, Task<EngineEntryPoint?>> _packageIdentityToEntryPt = new();
    private readonly ConstructEntryPoint _constructEntryPoint;
    private readonly NugetDownloader _nugetDownloader;

    public EntryPointCache(
        ConstructEntryPoint constructEntryPoint,
        NugetDownloader nugetDownloader)
    {
        _constructEntryPoint = constructEntryPoint;
        _nugetDownloader = nugetDownloader;
    }

    public async Task<EngineEntryPoint?> GetFor(SpriggitMeta meta, CancellationToken cancel)
    {
        Task<PackageIdentity?> identTask;
        lock (_lock)
        {
            if (!_metaToPackageIdentity.TryGetValue(meta, out identTask!))
            {
                identTask = _nugetDownloader.GetFirstIdentityFor(meta, cancel);
                _metaToPackageIdentity[meta] = identTask;
            }
        }

        var ident = await identTask;
        if (ident == null) return null;
        return await GetFor(ident, cancel);
    }

    public async Task<EngineEntryPoint?> GetFor(PackageIdentity? ident, CancellationToken cancel)
    {
        if (ident == null) return null;
        Task<EngineEntryPoint?> entryPt;
        lock (_lock)
        {
            if (!_packageIdentityToEntryPt.TryGetValue(ident, out entryPt!))
            {
                entryPt = _constructEntryPoint.ConstructFor(ident, cancel);
                _packageIdentityToEntryPt[ident] = entryPt;
            }
        }

        return await entryPt;
    }
}