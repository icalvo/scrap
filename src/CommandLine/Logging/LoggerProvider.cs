using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

public interface ILoggerBuilder<out TLogger, in TConfig>
{
    TLogger Build(TConfig config);
}

public class FunctionLoggerBuilder<TLogger, TConfig> : ILoggerBuilder<TLogger, TConfig>
{
    private readonly Func<TConfig, TLogger> _build;

    public FunctionLoggerBuilder(Func<TConfig, TLogger> build)
    {
        _build = build;
    }

    public TLogger Build(TConfig config)
    {
        return _build(config);
    }
}

public sealed class LoggerProvider<TLogger, TConfig> : ILoggerProvider
    where TLogger: ILogger where TConfig : class
{
    private readonly ILoggerBuilder<TLogger, TConfig> _loggerBuilder;
    private readonly TConfig _configuration;

    public LoggerProvider(IOptions<TConfig> options, ILoggerBuilder<TLogger, TConfig> loggerBuilder)
    {
        _loggerBuilder = loggerBuilder;
        _configuration = options.Value;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggerBuilder.Build(_configuration);
    }
 
    public void Dispose()
    {
    }
}
