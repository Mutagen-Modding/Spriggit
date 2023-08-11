using Noggog;

namespace Spriggit.Core;

public class SpriggitSource
{
    public string PackageName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

    public override string ToString()
    {
        if (Version.IsNullOrEmpty())
        {
            return PackageName;
        }
        else
        {
            return $"{PackageName}.{Version}";
        }
    }
}