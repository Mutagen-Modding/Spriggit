using Mutagen.Bethesda;

namespace Spriggit.Core;

public record SpriggitMeta(SpriggitSource Source, GameRelease Release);
public record SpriggitMetaSerialize(string? PackageName, string? Version, GameRelease? Release);