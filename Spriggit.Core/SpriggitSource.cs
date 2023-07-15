namespace Spriggit.Core;

public class SpriggitSource
{
    public string PackageName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{PackageName}{Version}";
    }
}