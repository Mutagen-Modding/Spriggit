using Mutagen.Bethesda;

namespace Spriggit.Engine.Services.Singletons;

public class DefaultSpriggitPackageNameGenerator
{
    public string PackageNameFor(string target, GameRelease release)
    {
        return $"Spriggit.{target}.{release.ToCategory()}";
    }
}