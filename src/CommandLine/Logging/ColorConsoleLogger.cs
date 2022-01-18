using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Scrap.CommandLine.Logging;

public class ColorConsoleLogger : ILogger
{
    private readonly ColorConfiguration _configuration;

    public ColorConsoleLogger(ColorConfiguration configuration)
    {
        _configuration = configuration;
    }

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

        var foregroundColor = _configuration.ColorFor(logLevel) ?? defForegroundColor;

        var backgroundColor = defBackgroundColor;

        string logRecord;
        logRecord = exception != null
            ? $"{formattedMessage}"
            : $"{formattedMessage} {exception?.ToStringDemystified() ?? ""}";
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
