using NuGet.Common;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace Spriggit.Engine.Services.Singletons;

public class NugetLoggerWrap : NuGet.Common.ILogger
{
    private readonly ILogger _logger;

    public NugetLoggerWrap(ILogger logger)
    {
        _logger = logger;
    }

    public void LogDebug(string data)
    {
        _logger.Debug(data);
    }

    public void LogVerbose(string data)
    {
        _logger.Debug(data);
    }

    public void LogInformation(string data)
    {
        _logger.Information(data);
    }

    public void LogMinimal(string data)
    {
        _logger.Information(data);
    }

    public void LogWarning(string data)
    {
        _logger.Warning(data);
    }

    public void LogError(string data)
    {
        _logger.Error(data);
    }

    public void LogInformationSummary(string data)
    {
        _logger.Information(data);
    }

    public void Log(LogLevel level, string data)
    {
        _logger.Write(ToLevel(level), data);
    }

    public async Task LogAsync(LogLevel level, string data)
    {
        _logger.Write(ToLevel(level), data);
    }

    private LogEventLevel ToLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => LogEventLevel.Information,
            LogLevel.Verbose => LogEventLevel.Information,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Minimal => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }

    public void Log(ILogMessage message)
    {
        _logger.Write(ToLevel(message.Level), "Testing {Code} {Path} {Message}", message.Code, message.ProjectPath, message.Message);
    }

    public async Task LogAsync(ILogMessage message)
    {
        Log(message);
    }
}
