using Serilog;

namespace Spriggit.UI.Services;

public class Startup
{
    private readonly ILogger _logger;

    public Startup(ILogger logger)
    {
        _logger = logger;
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

        var versionLine = $"============== Opening Spriggit v{AssemblyVersions.For<Startup>()} ==============";
        var bars = new string('=', versionLine.Length);
        _logger.Information(bars);
        _logger.Information(versionLine);
        _logger.Information(bars);
        _logger.Information(DateTime.Now.ToString());
        _logger.Information($"Start Path: {string.Join(' ', Environment.GetCommandLineArgs().FirstOrDefault())}");
        _logger.Information($"Args: {string.Join(' ', Environment.GetCommandLineArgs().Skip(1))}");
    }
}