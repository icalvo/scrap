using Microsoft.Extensions.Logging;

namespace Scrap.CommandLine.Logging;

public class FileLogger : ILogger
{
    private readonly string _fullFilePath;

    public FileLogger(FileLoggingConfiguration configuration)
    {
        _fullFilePath = Path.Combine(
            configuration.FolderPath,
            configuration.FilePath == null
                ? $"log{Guid.NewGuid():D}.txt"
                : configuration.FilePath
                    .Replace("{date}", DateTimeOffset.UtcNow.ToString("yyyyMMdd"))
                    .Replace("{guid}", Guid.NewGuid().ToString("D")));
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => new NullDisposable();

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var formattedMessage = formatter(state, exception);
        var logLevelRepr = logLevel switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "FATAL",
            LogLevel.None => "NONE",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };

        var logRecord =
            $"[{DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss+00:00}] [{logLevelRepr}] {formattedMessage} {exception?.StackTrace ?? ""}";

        using var streamWriter = new StreamWriter(_fullFilePath, true);
        streamWriter.WriteLine(logRecord);
    }

    private class NullDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
