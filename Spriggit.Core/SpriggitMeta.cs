﻿using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;

namespace Spriggit.Core;

public record SpriggitMeta(SpriggitSource Source, GameRelease Release);
public record SpriggitEmbeddedMeta(SpriggitSource Source, GameRelease Release, ModKey ModKey);
public record SpriggitMetaSerialize(string? PackageName, string? Version, GameRelease? Release);