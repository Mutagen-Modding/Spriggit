using Serilog;

namespace Spriggit.TranslationPackages;

public static class LoggerSetup
{
    public static ILogger Logger { get; }

    static LoggerSetup()
    {
        Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();
    }
}