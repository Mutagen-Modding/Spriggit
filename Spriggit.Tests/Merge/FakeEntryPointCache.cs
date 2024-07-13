using Noggog;
using NSubstitute;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Serilog;
using Spriggit.Core;
using Spriggit.Engine;
using Spriggit.Engine.Services.Singletons;

namespace Spriggit.Tests.Merge;

public class FakeEntryPointCache : IEntryPointCache
{
    private readonly Dictionary<SpriggitMeta, IEntryPoint> _dict = new();
    
    public void RegisterFor(SpriggitMeta meta, IEntryPoint entryPoint)
    {
        _dict[meta] = entryPoint;
    }
    
    public async Task<IEngineEntryPoint?> GetFor(SpriggitMeta meta, CancellationToken cancel)
    {
        var ep = _dict.GetOrDefault(meta);
        if (ep == null) return null;
        return new EngineEntryPointWrapper(
            Substitute.For<ILogger>(),
            new PackageIdentity("UnitTests", new NuGetVersion(1, 1, 1)),
            new EngineEntryPointWrapperItem(ep, null));
    }

    public Task<IEngineEntryPoint?> GetFor(PackageIdentity? ident, CancellationToken cancel)
    {
        throw new NotImplementedException();
    }
}