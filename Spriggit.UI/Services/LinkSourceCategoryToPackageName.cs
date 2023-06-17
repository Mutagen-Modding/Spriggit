using Mutagen.Bethesda;
using Noggog;
using Spriggit.UI.Settings;

namespace Spriggit.UI.Services;

public class LinkSourceCategoryToPackageName
{
    public GetResponse<string> GetPackageName(
        LinkSourceCategory sourceCategory,
        GameRelease release,
        string packageName)
    {
        if (packageName.IsNullOrWhitespace())
        {
            switch (sourceCategory)
            {
                case LinkSourceCategory.Json:
                    packageName = $"Spriggit.Json.{release.ToCategory()}";
                    break;
                case LinkSourceCategory.Yaml:
                    packageName = $"Spriggit.Yaml.{release.ToCategory()}";
                    break;
                case LinkSourceCategory.Custom:
                default:
                    return GetResponse<string>.Fail("No package to check");
            }
        }
        
        return GetResponse<string>.Succeed(packageName);
    }
}