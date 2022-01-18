using Microsoft.Extensions.Logging;

namespace Scrap.CommandLine.Logging;

public class ScrapConsoleLogger : ILogger
{
    public IDisposable BeginScope<TState>(TState state)
    {
        return new NullDisposable();
    }
 
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }
 
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }
        
        var formattedMessage = formatter(state, exception);

        var defForegroundColor = Console.ForegroundColor;
        var defBackgroundColor = Console.BackgroundColor;

        var foregroundColor = logLevel switch
        {
            LogLevel.Trace => ConsoleColor.Gray,
            LogLevel.Debug => ConsoleColor.Blue,
            LogLevel.Information => defForegroundColor,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Critical => ConsoleColor.Magenta,
            LogLevel.None => defBackgroundColor,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };

        var backgroundColor = defBackgroundColor;

        var logRecord = $"{formattedMessage} {exception?.StackTrace ?? ""}";
        Console.ForegroundColor = foregroundColor;
        Console.BackgroundColor = backgroundColor;
        Console.WriteLine(logRecord);
        Console.ForegroundColor = defForegroundColor;
        Console.BackgroundColor = defBackgroundColor;

    }
    
    private class NullDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }    
}
