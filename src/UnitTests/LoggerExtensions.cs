using Microsoft.Extensions.Logging;

namespace Scrap.Tests.Unit;

public static class LoggerExtensions
{
    public static ILogger<T> ToGeneric<T>(this ILogger logger) => new WrapLogger<T>(logger);

    private class WrapLogger<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public WrapLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            _logger.Log(logLevel, eventId, state, exception, formatter);

        public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _logger.BeginScope(state);
    }
}
