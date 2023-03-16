using LiteDB;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Caching;
using Polly.Caching.Memory;
using Scrap.Common.Logging;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Infrastructure.Factories;

namespace Scrap.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureInfrastructureServices(
        this IServiceCollection container,
        IConfiguration cfg,
        bool withConsole,
        bool verbose,
        IOAuthCodeGetter oAuthCodeGetter,
        IPageMarkerRepositoryFactory? pageMarkerRepoFactory = null,
        IFileSystemFactory? fileSystemFactory = null,
        IPageRetrieverFactory? pageRetrieverFactory = null,
        IDownloadStreamProviderFactory? downloadStreamProviderFactory = null)
    {
        container.AddSingleton(cfg);
        container.AddSingleton<IOAuthCodeGetter>(oAuthCodeGetter);
        container.Configure<DatabaseInfo>(cfg.GetSection("Scrap"));
        container.AddSingleton<DatabaseInfo>(sp => sp.GetRequiredService<IOptions<DatabaseInfo>>().Value);
        container.AddOptions<MemoryCacheOptions>();
        container.AddMemoryCache();
        container.AddLogging(o => ConfigureLogging(o, cfg, withConsole, verbose));

        container.AddSingleton<IAsyncCacheProvider, MemoryCacheProvider>();
        container.AddSingleton(
            sp =>
            {
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("InfrastructureServiceConfiguration");
                var config = sp.GetRequiredService<IConfiguration>();
                logger.LogDebug("Scrap DB: {ConnectionString}", config[ConfigKeys.Database]);
                return new ConnectionString(config[ConfigKeys.Database]);
            });
        container.AddSingleton<ILiteDatabase, LiteDatabase>();

        if (fileSystemFactory != null)
        {
            container.AddSingleton<IFileSystemFactory>(fileSystemFactory);
        }
        else
        {
            container.AddSingleton<IFileSystemFactory, FileSystemFactory>(
                sp =>
                {
                    var config = sp.GetRequiredService<IConfiguration>();
                    return new FileSystemFactory(
                        sp.GetRequiredService<IOAuthCodeGetter>(),
                        config[ConfigKeys.FileSystemType] ?? "local");
                });
        }

        if (pageRetrieverFactory != null)
        {
            container.AddSingleton(pageRetrieverFactory);
        }
        else
        {
            container.AddSingleton<IPageRetrieverFactory, PageRetrieverFactory>();
        }

        if (downloadStreamProviderFactory != null)
        {
            container.AddSingleton(downloadStreamProviderFactory);
        }
        else
        {
            container.AddSingleton<IDownloadStreamProviderFactory, DownloadStreamProviderFactory>();
        }

        container.AddSingleton<IAsyncPolicyFactory, AsyncPolicyFactory>();
        container.AddSingleton<IResourceRepositoryFactory, ResourceRepositoryFactory>(
            sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                return new ResourceRepositoryFactory(
                    config[ConfigKeys.BaseRootFolder],
                    sp.GetRequiredService<ILoggerFactory>(),
                    sp.GetRequiredService<IFileSystemFactory>());
            });
        container.AddSingleton<ILinkCalculatorFactory, LinkCalculatorFactory>();
        container
            .AddSingleton<IResourceRepositoryConfigurationValidatorFactory,
                ResourceRepositoryConfigurationValidatorFactory>();

        if (pageMarkerRepoFactory != null)
        {
            container.AddSingleton(pageMarkerRepoFactory);
        }
        else
        {
            container.AddSingleton<IPageMarkerRepositoryFactory, PageMarkerRepositoryFactory>();
        }

        return container;
    }

    public static IServiceCollection ConfigureInfrastructureServices(
        this IServiceCollection container,
        IConfigurationRoot config,
        IOAuthCodeGetter oAuthCodeGetter,
        IPageMarkerRepositoryFactory pageMarkerRepoFactory,
        IFileSystemFactory fileSystemFactory,
        IPageRetrieverFactory pageRetrieverFactory,
        IDownloadStreamProviderFactory downloadStreamProviderFactory)
    {
        return ConfigureInfrastructureServices(container, config, true, true, oAuthCodeGetter, pageMarkerRepoFactory, fileSystemFactory, pageRetrieverFactory, downloadStreamProviderFactory);
    }

    private static void ConfigureLogging(ILoggingBuilder builder, IConfiguration configuration, bool withConsole, bool verbose)
    {
        builder.ClearProviders();
        builder.AddConfiguration(configuration.GetSection("Logging"));
        var globalUserConfigFolder = GlobalConfig.GetGlobalUserConfigFolder(configuration);
        if (Directory.Exists(globalUserConfigFolder))
        {
            builder.AddFile(
                configuration.GetSection("Logging:File"),
                options => options.FolderPath = globalUserConfigFolder);
        }

        if (!verbose)
        {
            builder.AddFilter(level => level != LogLevel.Trace);
        }

        if (withConsole)
        {
            builder.AddConsole();
            builder.AddConsoleFormatter<CustomConsoleFormatter, CustomConsoleFormatterOptions>();
        }
    }
    
}

public static class GlobalConfig
{

    public static readonly string DefaultGlobalUserConfigFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".scrap");

    public static string GetGlobalUserConfigFolder(IConfiguration configuration) =>
        configuration[ConfigKeys.ConfigFolderEnvironment] ?? DefaultGlobalUserConfigFolder;
}
