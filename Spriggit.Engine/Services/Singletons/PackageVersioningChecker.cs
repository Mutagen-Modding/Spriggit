using Mutagen.Bethesda;
using NuGet.Packaging.Core;

namespace Spriggit.Engine.Services.Singletons;

public class PackageVersioningChecker
{
    private static readonly Version DataPathVersion = new Version(0, 26, 0);
    
    public bool IsSpriggitPackage(PackageIdentity packageIdentity)
    {
        var split = packageIdentity.Id.Split(".");
        if (split.Length != 3) return false;
        if (split[0] != "Spriggit") return false;
        if (split[1] != "Json" && split[1] != "Yaml") return false;
        if (!Enum.TryParse<GameRelease>(split[2], out var _)) return false;
        return true;
    }
    
    public bool SupportsDataPath(PackageIdentity packageIdentity)
    {
        if (!IsSpriggitPackage(packageIdentity)) return true;
        return packageIdentity.Version.Version >= DataPathVersion;
    }
}