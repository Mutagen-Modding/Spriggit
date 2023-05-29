using System.IO.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;

namespace Spriggit.UI.Settings;

public class SettingsSaver
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly ISaveMainSettings[] _saveMainSettings;
    
    public static readonly JsonSerializerSettings JsonSettings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        Converters =
        {
            new StringEnumConverter(),
        }
    };
    
    public SettingsSaver(
        IFileSystem fileSystem,
        ILogger logger,
        ISaveMainSettings[] saveMainSettings)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _saveMainSettings = saveMainSettings;
    }

    public void Save()
    {
        try
        {
            var settings = new MainSettings();
            foreach (var saver in _saveMainSettings)
            {
                saver.SaveInto(settings);
            }
            _fileSystem.File.WriteAllText(SettingsLoader.SettingsPath,
                JsonConvert.SerializeObject(settings, Formatting.Indented, JsonSettings));
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error saving settings");
        }
    }
}