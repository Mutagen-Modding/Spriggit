using Mutagen.Bethesda;

namespace Spriggit.UI.Settings;

public class LinkSettings
{
    public string ModPath { get; set; } = string.Empty;
    public string GitPath { get; set; } = string.Empty;
    public string DataFolderPath { get; set; } = string.Empty;
    public GameRelease GameRelease { get; set; } = GameRelease.SkyrimSE;
    public LinkSourceCategory SourceCategory { get; set; }
    public string SpriggitPackageName { get; set; } = string.Empty;
    public string SpriggitPackageVersion { get; set; } = string.Empty;
    public bool ThrowOnUnknown { get; set; } = true;
}
