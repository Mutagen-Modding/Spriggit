using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;

namespace Spriggit.Core;

public record SpriggitFile(
    SpriggitMeta? Meta,
    KnownMaster[] KnownMasters)
{
    public override string ToString()
    {
        return $"{nameof(SpriggitFile)} {{ {nameof(Meta)} = {Meta?.ToString() ?? "null"}, " +
               $"{nameof(KnownMasters)} = {FormatKnownMasters(KnownMasters)} }}";
    }

    internal static string FormatKnownMasters(KnownMaster[]? knownMasters)
    {
        if (knownMasters == null) return "null";
        return $"[{string.Join(", ", knownMasters.Select(x => x.ToString()))}]";
    }
}

public record KnownMaster(
    ModKey ModKey,
    MasterStyle Style);

public record SpriggitFileSerialize(
    string? PackageName,
    string? Version,
    GameRelease? Release,
    KnownMaster[]? KnownMasters)
{
    public override string ToString()
    {
        return $"{nameof(SpriggitFileSerialize)} {{ {nameof(PackageName)} = {PackageName ?? "null"}, " +
               $"{nameof(Version)} = {Version ?? "null"}, " +
               $"{nameof(Release)} = {Release?.ToString() ?? "null"}, " +
               $"{nameof(KnownMasters)} = {SpriggitFile.FormatKnownMasters(KnownMasters)} }}";
    }
}

public record SpriggitMeta(SpriggitSource Source, GameRelease Release);

public record SpriggitModKeyMeta(SpriggitSource Source, GameRelease Release, ModKey ModKey)
{
    public SpriggitMeta ToMeta() => new SpriggitMeta(Source, Release);
}

public record SpriggitModKeyMetaSerialize(
    string? PackageName, string? Version, GameRelease? Release, string? ModKey);
