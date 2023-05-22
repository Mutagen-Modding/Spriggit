using Mutagen.Bethesda.Plugins.Records;
using Spriggit.Core;

namespace Spriggit.Engine;

public class EntryPointCache
{
    private readonly Dictionary<SpriggitMeta, Task<IEntryPoint<IMod, IModGetter>?>> _entryPoints = new();
    private readonly ConstructEntryPoint _constructEntryPoint;
    private readonly GetDefaultEntryPoint _getDefaultEntryPoint;

    public EntryPointCache(
        ConstructEntryPoint constructEntryPoint,
        GetDefaultEntryPoint getDefaultEntryPoint)
    {
        _constructEntryPoint = constructEntryPoint;
        _getDefaultEntryPoint = getDefaultEntryPoint;
    }

    public async Task<IEntryPoint<IMod, IModGetter>?> GetFor(SpriggitMeta meta)
    {
        Task<IEntryPoint<IMod, IModGetter>?> entryPt;
        lock (_entryPoints)
        {
            if (!_entryPoints.TryGetValue(meta, out entryPt!))
            {
                entryPt = _constructEntryPoint.ConstructFor(meta, CancellationToken.None);
                _entryPoints[meta] = entryPt;
            }
        }

        return await entryPt;
    }

    public async Task<IEntryPoint> GetDefault(string spriggitPluginPath)
    {
        // ToDo
        // Cache entry point
        return await _getDefaultEntryPoint.Get(spriggitPluginPath);
    }
}