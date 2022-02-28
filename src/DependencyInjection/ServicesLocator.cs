using System.Net;
using LazyProxy.ServiceProvider;
using LiteDB;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Scrap.Application;
using Scrap.Application.Scrap;
using Scrap.Domain;
using Scrap.Domain.Downloads;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.JobDefinitions.JsonFile;
using Scrap.Domain.Jobs;
using Scrap.Domain.Jobs.Graphs;
using Scrap.Domain.Pages;
using Scrap.Domain.Pages.LiteDb;
using Scrap.Domain.Resources;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.DependencyInjection;

public class ServicesLocator
{
    private static readonly TimeSpan DefaultCacheTtl = TimeSpan.FromMinutes(5);
    private readonly IServiceProvider _serviceProvider;

    public ServicesLocator(IConfiguration config, Action<ILoggingBuilder> configureLogging)
    {
        IServiceCollection container = new ServiceCollection();
        ConfigureServices(config, container, configureLogging);

        _serviceProvider = container.BuildServiceProvider();
    }

    public T Get<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    private static void ConfigureServices(IConfiguration cfg, IServiceCollection container,
        Action<ILoggingBuilder> configureLogging)
    {
        container.AddSingleton(cfg);
        container.AddOptions<MemoryCacheOptions>();
        container.AddMemoryCache();
        container.AddLogging(configureLogging);


        container.AddTransient<JobDefinitionsApplicationService>();
        container.AddTransient<ScrapApplicationService>();
        container.AddLazyTransient<IScrapDownloadsService, ScrapDownloadsService>();
        container.AddLazyTransient<IScrapTextService, ScrapTextService>();
        container.AddLazyTransient<IDownloadApplicationService, DownloadApplicationService>();
        container.AddLazyTransient<ITraversalApplicationService, TraversalApplicationService>();
        container.AddLazyTransient<IResourcesApplicationService, ResourcesApplicationService>();

        container.AddSingleton<IAsyncCacheProvider, MemoryCacheProvider>();
        container.AddSingleton<FullScanLinkCalculator>();
        container.AddSingleton<LinkCalculator>();
        container.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<ServicesLocator>>();
            var config = sp.GetRequiredService<IConfiguration>();
            logger.LogDebug("Scrap DB: {ConnectionString}", config["Scrap:Database"]);
            return new ConnectionString(config["Scrap:Database"]);
        });
        container.AddSingleton<ILiteDatabase, LiteDatabase>();
        container.AddSingleton<IEntityRegistry<Job>, EntityRegistry<Job>>();
        container.AddSingleton<IEntityRegistry<JobDefinition>, EntityRegistry<JobDefinition>>();
        container.AddSingleton(GetJob);
        container.AddSingleton(BuildLinkCalculator);
        container.AddSingleton<IJobFactory, JobFactory>();
        container.AddSingleton<IJobDefinitionRepository>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<ServicesLocator>>();
            var config = sp.GetRequiredService<IConfiguration>();
            logger.LogDebug("Definitions file: {DefinitionsPath}", config["Scrap:Definitions"]);
            return new MemoryJobDefinitionRepository(config["Scrap:Definitions"]);
        });
        container.AddSingleton<IGraphSearch, DepthFirstGraphSearch>();
        container.AddSingleton<IPageRetriever, HttpPageRetriever>();
        container.AddSingleton<IPageMarkerRepository, LiteDbPageMarkerRepository>();
        container.AddSingleton<IPageRetriever, HttpPageRetriever>();
        container.AddTransient(sp => (FileSystemResourceRepositoryConfiguration)GetJobResourceRepoArgs(sp));
        container.AddLazyTransient<IDestinationProvider, CompiledDestinationProvider>();
        container.AddLazySingleton<IResourceRepositoryConfigurationValidator, FileSystemResourceRepositoryConfigurationValidator>();
        container.AddLazyTransient<IResourceRepository, FileSystemResourceRepository>(sp =>
            new FileSystemResourceRepository(
                sp.GetRequiredService<IDestinationProvider>(),
                sp.GetRequiredService<FileSystemResourceRepositoryConfiguration>(),
                sp.GetRequiredService<ILogger<FileSystemResourceRepository>>(),
                GetJob(sp).DisableResourceWrites));
        container.AddSingleton(BuildDownloadStreamProvider);
        container.AddSingleton(BuildAsyncPolicy);
    }

    private static Job GetJob(IServiceProvider sp)
    {
        return sp.GetRequiredService<IEntityRegistry<Job>>().RegisteredEntity ?? throw new Exception("The job is not registered yet.");
    }

    private static ILinkCalculator BuildLinkCalculator(IServiceProvider sp)
    {
        return sp.GetRequiredService<Job>().FullScan
            ? sp.GetRequiredService<FullScanLinkCalculator>()
            : sp.GetRequiredService<LinkCalculator>();
    }

    private static IResourceRepositoryConfiguration GetJobResourceRepoArgs(IServiceProvider sp)
    {
        var jobRegistry = sp.GetService<IEntityRegistry<Job>>();
        var jobDefinitionRegistry = sp.GetService<IEntityRegistry<JobDefinition>>();

        if (jobRegistry?.RegisteredEntity != null)
        {
            return jobRegistry.RegisteredEntity.ResourceRepoArgs;
        }

        if (jobDefinitionRegistry?.RegisteredEntity != null)
        {
            return jobDefinitionRegistry.RegisteredEntity.ResourceRepoArgs;
        }

        throw new Exception("Neither a job or job definition is registered yet");
    }

    private static IAsyncPolicy BuildAsyncPolicy(IServiceProvider sp)
    {
        static bool IsClientError(Exception ex)
            => ex is HttpRequestException { StatusCode: >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError };

        var job = sp.GetRequiredService<Job>();
        var cacheProvider = sp.GetRequiredService<IAsyncCacheProvider>();
        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
        int httpRequestRetries = job.HttpRequestRetries;
        TimeSpan httpDelay = job.HttpRequestDelayBetweenRetries;
        var cacheLogger = loggerFactory.CreateLogger("Cache");
        var cachePolicy = Policy.CacheAsync(
            cacheProvider,
            DefaultCacheTtl,
            (_, key) => { cacheLogger.LogRequest("CACHED", key); },
            (_, _) => { },
            (_, _) => { },
            (_, _, _) => { },
            (_, _, _) => { });
        var retryPolicy = Policy.Handle<Exception>(ex => !IsClientError(ex))
            .WaitAndRetryAsync(
                httpRequestRetries,
                _ => TimeSpan.Zero,
                (exception, _) => { Console.WriteLine(exception.Message); });

        return Policy.WrapAsync(cachePolicy, retryPolicy, AsyncDelayPolicy.Create(httpDelay));
    }

    private static IDownloadStreamProvider BuildDownloadStreamProvider(IServiceProvider sp)
    {
        var policy = sp.GetRequiredService<IAsyncPolicy>();
        var protocol = "http";
        var providerLogger = sp.GetRequiredService<ILogger<HttpClientDownloadStreamProvider>>();
        return BuildDownloadStreamProvider(policy, protocol, providerLogger);
    }

    private static IResourceRepository BuildResourceRepository(IServiceProvider sp)
    {
        var job = sp.GetRequiredService<Job>();
        var resourceRepoArgs = job.ResourceRepoArgs;
        var repos = sp.GetServices<IResourceRepository>().ToDictionary(x => x.GetType().Name);

        if (repos.TryGetValue(resourceRepoArgs.RepositoryType, out var repo))
        {
            return repo;
        }

        throw new InvalidOperationException(
            $"Unknown resource processor config type: {resourceRepoArgs.GetType().Name}");
    }
    
    private static IDownloadStreamProvider BuildDownloadStreamProvider(
        IAsyncPolicy policy,
        string protocol,
        ILogger<HttpClientDownloadStreamProvider> logger)
    {
        DelegatingHandler[] wrappingHandlers =
        {
            new PollyMessageHandler(policy),
            new LoggingHandler(logger)
        };
        HttpMessageHandler primaryHandler = new HttpClientHandler();

        var handler = wrappingHandlers.Reverse().Aggregate(primaryHandler, (accum, item) =>
        {
            item.InnerHandler = accum;
            return item;
        });

        switch (protocol)
        {
            case "http":
            case "https":
                var httpClient = new HttpClient(handler);
                return new HttpClientDownloadStreamProvider(httpClient);
            default:
                throw new ArgumentException($"Unknown URI protocol {protocol}", nameof(protocol));
        }
    }
    
    private class PollyMessageHandler: DelegatingHandler
    {
        private readonly IAsyncPolicy _policy;

        public PollyMessageHandler(IAsyncPolicy policy)
        {
            _policy = policy;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            _policy.ExecuteAsync(
                (_, ct) => base.SendAsync(request, ct),
                new Context(request.RequestUri?.AbsoluteUri), cancellationToken);
    }

    private class LoggingHandler: DelegatingHandler
    {
        private readonly ILogger _logger;

        public LoggingHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.LogRequest(request.Method.ToString(), request.RequestUri?.AbsoluteUri);
            return base.SendAsync(request, cancellationToken);
        }
    }    
}
