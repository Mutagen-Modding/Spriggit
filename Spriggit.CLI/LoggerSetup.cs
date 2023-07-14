using Serilog;

namespace Spriggit.CLI;

public static class LoggerSetup
{
    public static ILogger Logger { get; }

    static LoggerSetup()
    {
        Logger = new LoggerConfiguration()
            .CreateLogger();
    }
}