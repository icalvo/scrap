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
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Domain.Sites;
using Scrap.Infrastructure.Factories;
using Scrap.Infrastructure.Repositories;

namespace Scrap.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureLogging(
        this IServiceCollection container,
        IConfiguration cfg,
        bool withConsole,
        bool verbose)
    {
        container.AddLogging(o => ConfigureLogging(o, cfg, withConsole, verbose));
        return container;
    }

    public static IServiceCollection ConfigureInfrastructureServices(
        this IServiceCollection container,
        IConfiguration cfg,
        IOAuthCodeGetter oAuthCodeGetter,
        IVisitedPageRepositoryFactory? visitedPageRepositoryFactory = null,
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

        container.AddSingleton<IAsyncCacheProvider, MemoryCacheProvider>();
        container.AddSingleton(
            sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                return new ConnectionString(config.Database());
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
                        config.FileSystemType() ?? "local",
                        sp.GetRequiredService<ILogger<FileSystemFactory>>());
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
                    config.BaseRootFolder(),
                    sp.GetRequiredService<ILoggerFactory>(),
                    sp.GetRequiredService<IFileSystemFactory>(),
                    sp.GetRequiredService<IDestinationProviderFactory>(),
                    sp.GetRequiredService<ILogger<ResourceRepositoryFactory>>());
            });
        container.AddSingleton<ILinkCalculatorFactory, LinkCalculatorFactory>();
        container.AddSingleton<IResourceRepositoryConfigurationValidator, ResourceRepositoryConfigurationValidator>();

        if (visitedPageRepositoryFactory != null)
        {
            container.AddSingleton(visitedPageRepositoryFactory);
        }
        else
        {
            container.AddSingleton<IVisitedPageRepositoryFactory, VisitedPageRepositoryFactory>();
        }

        container.AddSingleton<IDestinationProviderFactory, DestinationProviderFactory>();

        container.AddSingleton<ISiteRepository>(
            sp =>
            {
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("InfrastructureServiceConfiguration");
                var config = sp.GetRequiredService<IConfiguration>();
                var sitesFilePath = config.Sites();
                if (sitesFilePath == null)
                {
                    throw new Exception("No definitions file in the configuration!");
                }

                logger.LogTrace("Definitions file: {DefinitionsPath}", sitesFilePath);
                return new MemorySiteRepository(
                    sitesFilePath,
                    sp.GetRequiredService<IFileSystemFactory>(),
                    sp.GetRequiredService<ILogger<MemorySiteRepository>>());
            });        
        
        return container;
    }

    private static void ConfigureLogging(ILoggingBuilder builder, IConfiguration configuration, bool withConsole, bool verbose)
    {
        builder.ClearProviders();
        builder.AddConfiguration(configuration.GetSection("Logging"));

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
