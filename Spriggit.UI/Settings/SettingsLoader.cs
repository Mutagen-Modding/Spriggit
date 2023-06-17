using System.IO.Abstractions;
using Newtonsoft.Json;
using Spriggit.UI.Services;

namespace Spriggit.UI.Settings;

public class SettingsLoader : IStartupTask
{
    private readonly SettingsSingleton _settingsSingleton;
    private readonly IFileSystem _fileSystem;
    private readonly ISaveMainSettings[] _savers;
    public const string SettingsPath = "Settings.json";

    public SettingsLoader(
        SettingsSingleton settingsSingleton,
        IFileSystem fileSystem,
        ISaveMainSettings[] savers)
    {
        _settingsSingleton = settingsSingleton;
        _fileSystem = fileSystem;
        _savers = savers;
    }
    
    public async Task Start()
    {
        if (_fileSystem.File.Exists(SettingsPath))
        {
            _settingsSingleton.MainSettings = JsonConvert.DeserializeObject<MainSettings>(
                _fileSystem.File.ReadAllText(SettingsPath),
                new JsonSerializerSettings())!;
            foreach (var saver in _savers)
            {
                saver.ReadFrom(_settingsSingleton.MainSettings);
            }
        }
        else
        {
            _settingsSingleton.MainSettings = new();
        }
    }
}