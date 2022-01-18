using Microsoft.Extensions.Logging;

namespace Scrap.CommandLine.Logging;

public sealed class LoggerProvider<TLogger> : ILoggerProvider
    where TLogger: ILogger, new()
{
    public ILogger CreateLogger(string categoryName)
    {
        return new TLogger();
    }
 
    public void Dispose()
    {
    }
}
