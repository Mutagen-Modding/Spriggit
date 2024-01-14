using Serilog;

namespace Spriggit.CLI;

public static class LoggerSetup
{
    public static ILogger Logger { get; }

    static LoggerSetup()
    {
        Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
}