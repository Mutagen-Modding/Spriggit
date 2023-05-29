using System.ComponentModel;
using System.Windows;
using Autofac;
using Serilog;
using Spriggit.UI.Settings;

namespace Spriggit.UI.Services;

public class Shutdown
{
    private readonly ILifetimeScope _scope;
    private readonly SettingsSaver _settingsSaver;
    private readonly ILogger _logger;
    private readonly StartupTracker _startupTracker;
    private readonly IMainWindow _window;

    public bool IsShutdown { get; private set; }

    public Shutdown(
        ILogger logger,
        StartupTracker startupTracker,
        IMainWindow window,
        ILifetimeScope scope,
        SettingsSaver settingsSaver)
    {
        _logger = logger;
        _startupTracker = startupTracker;
        _window = window;
        _scope = scope;
        _settingsSaver = settingsSaver;
    }
    
    public void Prepare()
    {
        _window.Closing += (_, b) =>
        {
            _window.Visibility = Visibility.Collapsed;
            Closing(b);
        };
    }
        
    private async void ExecuteShutdown()
    {
        IsShutdown = true;

        await Task.Run(() =>
        {
            if (!_startupTracker.Initialized)
            {
                _logger.Information("App was unable to start up.  Not saving settings");
                return;
            }

            try
            {
                _settingsSaver.Save();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error saving settings");
            }
        });
            
        var toDo = new List<Task>();

        toDo.Add(Task.Run(() =>
        {
            try
            {
                _logger.Information("Disposing container");
                _scope.Dispose();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error shutting down container actions");
            }
        }));
        await Task.WhenAll(toDo);
        Application.Current.Shutdown();
    }

    private void Closing(CancelEventArgs args)
    {
        if (IsShutdown) return;
        args.Cancel = true;
        ExecuteShutdown();
    }
}
