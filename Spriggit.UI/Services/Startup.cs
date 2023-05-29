using System.Windows;
using Serilog;
using Spriggit.UI.ViewModels;

namespace Spriggit.UI.Services;

public class Startup
{
    private readonly ILogger _logger;
    private readonly IEnumerable<IStartupTask> _startupTasks;
    private readonly IMainWindow _window;
    private readonly Shutdown _shutdown;
    private readonly Lazy<MainVm> _mainVm;
    private readonly StartupTracker _startupTracker;

    public Startup(
        ILogger logger,
        IEnumerable<IStartupTask> startupTasks,
        IMainWindow window,
        Shutdown shutdown,
        Lazy<MainVm> mainVm,
        StartupTracker startupTracker)
    {
        _logger = logger;
        _startupTasks = startupTasks;
        _window = window;
        _shutdown = shutdown;
        _mainVm = mainVm;
        _startupTracker = startupTracker;
    }
    
    public void Initialize()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            _logger.Error(ex, "Crashing");
            while (ex?.InnerException != null)
            {
                ex = ex.InnerException;
                _logger.Error(ex, "Inner Exception");
            }
        };

        var assemb = AssemblyVersions.For<Startup>();
        var versionLine = $"============== Opening Spriggit v{assemb.ProductVersion} ==============";
        var bars = new string('=', versionLine.Length);
        _logger.Information(bars);
        _logger.Information(versionLine);
        _logger.Information(bars);
        _logger.Information(DateTime.Now.ToString());
        _logger.Information($"Start Path: {string.Join(' ', Environment.GetCommandLineArgs().FirstOrDefault())}");
        _logger.Information($"Args: {string.Join(' ', Environment.GetCommandLineArgs().Skip(1))}");
        
        try
        {
            Task.WhenAll(_startupTasks.Select(x => Task.Run(() => x.Start()))).Wait();
            _logger.Information("Loading settings"); 
            _mainVm.Value.Load();
            _logger.Information("Loaded settings");
            _logger.Information("Setting Main VM");
            _window.DataContext = _mainVm.Value;
            _logger.Information("Set Main VM");
            _logger.Information("Initializing Main VM");
            _mainVm.Value.Init();
            _logger.Information("Initialized Main VM");
            _startupTracker.Initialized = true;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error initializing app");
            Application.Current.Shutdown();
        }
            
        _shutdown.Prepare();
    }
}