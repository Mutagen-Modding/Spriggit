namespace Spriggit.UI.Settings;

public class MainSettings
{
    public LinkSettings AddNewLinkSettings { get; set; } = new();
    public LinkSettings[] Links { get; set; } = Array.Empty<LinkSettings>();
}
