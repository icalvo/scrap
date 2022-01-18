using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Scrap.CommandLine.Logging;

public static class FileLoggerExtensions
{
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, IConfigurationSection configurationSection,
        Action<FileLoggingConfiguration>? configure = null)
    {
        builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
        builder.Services.Configure<FileLoggingConfiguration>(configurationSection);
        builder.Services.Configure<FileLoggingConfiguration>((configuration => configure?.Invoke(configuration)));
        return builder;
    }

    public static ILoggingBuilder AddGeneric<TLogger>(this ILoggingBuilder builder)
        where TLogger : ILogger, new()
    {
        builder.Services.AddSingleton<ILoggerProvider, LoggerProvider<TLogger>>();
        return builder;
    }

    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, Action<FileLoggingConfiguration> configure)
    {
        builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
        builder.Services.Configure(configure);
        return builder;
    }
}
