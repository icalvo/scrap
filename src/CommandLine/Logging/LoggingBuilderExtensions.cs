using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Scrap.CommandLine.Logging;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddFile(
        this ILoggingBuilder builder,
        IConfigurationSection? configurationSection,
        Action<FileLoggingConfiguration>? configure = null)
    {
        builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
        if (configurationSection != null)
        {
            builder.Services.Configure<FileLoggingConfiguration>(configurationSection);
        }
        builder.Services.Configure<FileLoggingConfiguration>((configuration => configure?.Invoke(configuration)));
        return builder;
    }

    public static ILoggingBuilder AddGeneric<TLogger>(this ILoggingBuilder builder)
        where TLogger : ILogger, new()
    {
        builder.Services.AddSingleton<ILoggerProvider, LoggerProvider<TLogger>>();
        return builder;
    }

    public static ILoggingBuilder AddGeneric<TLogger, TConfig>(
        this ILoggingBuilder builder,
        Func<TConfig, TLogger> constructor,
        IConfigurationSection? configurationSection,
        Action<TConfig>? configure = null)
        where TLogger : ILogger
        where TConfig : class
    {
        builder.Services.AddSingleton<ILoggerBuilder<TLogger, TConfig>>(new FunctionLoggerBuilder<TLogger, TConfig>(constructor));
        builder.Services.AddSingleton<ILoggerProvider, LoggerProvider<TLogger, TConfig>>();
        if (configurationSection != null)
        {
            builder.Services.Configure<TConfig>(configurationSection);
        }

        if (configure != null)
        {
            builder.Services.Configure(configure);
        }

        return builder;
    }
}
