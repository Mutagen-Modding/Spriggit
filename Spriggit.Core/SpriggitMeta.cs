using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;

namespace Spriggit.Core;

public record SpriggitMeta(SpriggitSource Source, GameRelease Release);
    
public record SpriggitMetaSerialize(string? PackageName, string? Version, GameRelease? Release);


public record SpriggitModKeyMeta(SpriggitSource Source, GameRelease Release, ModKey ModKey)
{
    public SpriggitMeta ToMeta() => new SpriggitMeta(Source, Release);
}

public record SpriggitModKeyMetaSerialize(
    string? PackageName, string? Version, GameRelease? Release, string? ModKey);