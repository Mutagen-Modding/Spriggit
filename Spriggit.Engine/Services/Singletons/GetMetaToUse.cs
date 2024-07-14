using System.Data;
using Noggog;
using Spriggit.Core;

namespace Spriggit.Engine.Services.Singletons;

public class GetMetaToUse
{
    private readonly SpriggitExternalMetaPersister _externalMetaPersister;

    public GetMetaToUse(
        SpriggitExternalMetaPersister externalMetaPersister)
    {
        _externalMetaPersister = externalMetaPersister;
    }
    
    public async Task<SpriggitEmbeddedMeta> Get(
        SpriggitSource? source,
        DirectoryPath spriggitPluginPath,
        CancellationToken cancel)
    {
        var sourceInfo = _externalMetaPersister.TryParseEmbeddedMeta(spriggitPluginPath);

        if (sourceInfo == null) throw new DataException($"Could not locate source info from {spriggitPluginPath}");

        return new SpriggitEmbeddedMeta(
            ModKey: sourceInfo.ModKey,
            Source: source ?? sourceInfo.Source,
            Release: sourceInfo.Release);
    }
}