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
}
