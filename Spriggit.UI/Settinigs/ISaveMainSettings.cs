namespace Spriggit.UI.Settings;

public interface ISaveMainSettings
{
    void ReadFrom(MainSettings settings);
    void SaveInto(MainSettings settings);
}