using LiteDB;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using Scrap.Application;
using Scrap.Application.Scrap;
using Scrap.DependencyInjection.Factories;
using Scrap.Domain;
using Scrap.Domain.Downloads;
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
    private readonly IServiceProvider _serviceProvider;

    public ServicesLocator(IConfiguration config, Action<ILoggingBuilder> configureLogging)
    {
        IServiceCollection container = new ServiceCollection();
        ConfigureServices(config, container, configureLogging);

        _serviceProvider = container.BuildServiceProvider();
    }

    public T Get<T>() where T : notnull => _serviceProvider.GetRequiredService<T>();

    private static void ConfigureServices(
        IConfiguration cfg,
        IServiceCollection container,
        Action<ILoggingBuilder> configureLogging)
    {
        container.AddSingleton(cfg);
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
        container.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<ServicesLocator>>();
            var config = sp.GetRequiredService<IConfiguration>();
            logger.LogDebug("Scrap DB: {ConnectionString}", config["Scrap:Database"]);
            return new ConnectionString(config["Scrap:Database"]);
        });
        container.AddSingleton<ILiteDatabase, LiteDatabase>();
        container.AddSingleton<IJobDefinitionRepository>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<ServicesLocator>>();
            var config = sp.GetRequiredService<IConfiguration>();
            var definitionsFilePath = config["Scrap:Definitions"];
            if (definitionsFilePath == null)
            {
                throw new Exception("No definitions file in the configuration!");
            }

            logger.LogDebug("Definitions file: {DefinitionsPath}", definitionsFilePath);
            return new MemoryJobDefinitionRepository(
                definitionsFilePath,
                sp.GetRequiredService<ILogger<MemoryJobDefinitionRepository>>());
        });
        container.AddSingleton<IGraphSearch, DepthFirstGraphSearch>();
        container
            .AddSingleton<IResourceRepositoryConfigurationValidator,
                FileSystemResourceRepositoryConfigurationValidator>();

        container.AddSingleton<IVisitedPagesApplicationService, VisitedPagesApplicationService>();

        container.AddAsyncFactory<JobDto, Job, JobFactory>();
        container.AddFactory<Job, IPageRetriever, PageRetrieverFactory>();
        container.AddFactory<Job, IDownloadStreamProvider, DownloadStreamProviderFactory>();
        container.AddFactory<Job, AsyncPolicyConfiguration, IAsyncPolicy, AsyncPolicyFactory>();
        container.AddFactory<Job, IResourceRepository, ResourceRepositoryFactory>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            return new ResourceRepositoryFactory(config["Scrap:BaseRootFolder"],
                sp.GetRequiredService<ILoggerFactory>());
        });
        container.AddSingleton<IFactory<Job, ILinkCalculator>, LinkCalculatorFactory>();
        container
            .AddSingleton<IFactory<IResourceRepositoryConfiguration, IResourceRepositoryConfigurationValidator>,
                ResourceRepositoryConfigurationValidatorFactory>();

        container.AddOptionalFactory<Job, IPageMarkerRepository, PageMarkerRepositoryFactory>();
    }
}
