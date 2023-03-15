using LiteDB;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Caching;
using Polly.Caching.Memory;
using Scrap.Application;
using Scrap.Application.Scrap;
using Scrap.Common;
using Scrap.DependencyInjection.Factories;
using Scrap.Domain;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.JobDefinitions.JsonFile;
using Scrap.Domain.Jobs;
using Scrap.Domain.Jobs.Graphs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.DependencyInjection;

public class ServicesLocator
{
    public static IServiceProvider Build(
        IConfiguration config,
        Action<ILoggingBuilder> configureLogging,
        IOAuthCodeGetter oauthCodeGetter)
    {
        IServiceCollection container = new ServiceCollection();
        ConfigureServices(config, container, configureLogging, oauthCodeGetter);
        return container.BuildServiceProvider();
    }

    private static void ConfigureServices(
        IConfiguration cfg,
        IServiceCollection container,
        Action<ILoggingBuilder> configureLogging,
        IOAuthCodeGetter oauthCodeGetter)
    { 

        container.AddSingleton(cfg);
        container.Configure<DatabaseInfo>(cfg.GetSection("Scrap"));
        container.AddSingleton<DatabaseInfo>(sp => sp.GetRequiredService<IOptions<DatabaseInfo>>().Value);
        container.AddOptions<MemoryCacheOptions>();
        container.AddMemoryCache();
        container.AddLogging(configureLogging);

        container.AddTransient<JobDefinitionsApplicationService>();
        container.AddTransient<ScrapApplicationService>();
        container.AddTransient<IScrapDownloadsService, ScrapDownloadsService>();
        container.AddTransient<IScrapTextService, ScrapTextService>();
        container.AddTransient<IDownloadApplicationService, DownloadApplicationService>();
        container.AddTransient<ITraversalApplicationService, TraversalApplicationService>();
        container.AddTransient<IResourcesApplicationService, ResourcesApplicationService>();

        container.AddSingleton<IAsyncCacheProvider, MemoryCacheProvider>();
        container.AddSingleton(
            sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServicesLocator>>();
                var config = sp.GetRequiredService<IConfiguration>();
                logger.LogDebug("Scrap DB: {ConnectionString}", config[ConfigKeys.Database]);
                return new ConnectionString(config[ConfigKeys.Database]);
            });
        container.AddSingleton<ILiteDatabase, LiteDatabase>();
        container.AddSingleton<IJobDefinitionRepository>(
            sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServicesLocator>>();
                var config = sp.GetRequiredService<IConfiguration>();
                var definitionsFilePath = config[ConfigKeys.Definitions];
                if (definitionsFilePath == null)
                {
                    throw new Exception("No definitions file in the configuration!");
                }

                logger.LogDebug("Definitions file: {DefinitionsPath}", definitionsFilePath);
                return new MemoryJobDefinitionRepository(
                    definitionsFilePath,
                    sp.GetRequiredService<IFileSystemFactory>(),
                    sp.GetRequiredService<ILogger<MemoryJobDefinitionRepository>>());
            });
        container.AddSingleton<IGraphSearch, DepthFirstGraphSearch>();
        container
            .AddSingleton<IResourceRepositoryConfigurationValidator,
                FileSystemResourceRepositoryConfigurationValidator>();

        container.AddSingleton<IVisitedPagesApplicationService, VisitedPagesApplicationService>();
        container.AddSingleton<IOAuthCodeGetter>(oauthCodeGetter);
        container.AddSingleton<IJobFactory, JobFactory>();
        container.AddSingleton<IFileSystemFactory, FileSystemFactory>(
            sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                return new FileSystemFactory(
                    sp.GetRequiredService<IOAuthCodeGetter>(),
                    config[ConfigKeys.FileSystemType] ?? "local");
            });
        container.AddSingleton<IPageRetrieverFactory, PageRetrieverFactory>();
        container.AddSingleton<IDownloadStreamProviderFactory, DownloadStreamProviderFactory>();
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

        container.AddSingleton<IPageMarkerRepositoryFactory, PageMarkerRepositoryFactory>();
    }
}
