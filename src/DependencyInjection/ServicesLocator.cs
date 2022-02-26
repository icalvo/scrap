using System.Net;
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
    
    public ServicesLocator(ILoggerFactory loggerFactory, IConfiguration config)
    {
        ILogger logger = loggerFactory.CreateLogger<ServicesLocator>();
        logger.LogDebug("Scrap DB: {ConnectionString}", config["Scrap:Database"]);
        
        IServiceCollection container = new ServiceCollection();
        ConfigureServices(config, container);

        _serviceProvider = container.BuildServiceProvider();
    }

    public T Get<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    private static void ConfigureServices(IConfiguration config, IServiceCollection container)
    {
        container.AddOptions<MemoryCacheOptions>();
        container.AddMemoryCache();
        container.AddSingleton<IAsyncCacheProvider, MemoryCacheProvider>();
        container.AddSingleton<FullScanLinkCalculator>();
        container.AddSingleton<LinkCalculator>();
        container.AddSingleton(new ConnectionString(config["Scrap:Database"]));
        container.AddSingleton<ILiteDatabase, LiteDatabase>();
        container.AddTransient<IEntityRegistry<Job>, EntityRegistry<Job>>();
        container.AddTransient(GetJob);
        container.AddTransient(BuildLinkCalculator);
        container.AddTransient<ScrapApplicationService>();
        container.AddSingleton<JobDefinitionsApplicationService>();
        container.AddSingleton<IGraphSearch, DepthFirstGraphSearch>();
        container.AddSingleton<IPageRetriever, HttpPageRetriever>();
        container.AddSingleton<IPageRetriever, HttpPageRetriever>();
        container.AddSingleton<IPageMarkerRepository, LiteDbPageMarkerRepository>();
        container.AddSingleton<IPageRetriever, HttpPageRetriever>();
        container.AddSingleton<IDestinationProvider, CompiledDestinationProvider>();
        container.AddSingleton(BuildResourceRepositoryConfigurationValidator);
        container.AddSingleton<CompiledDestinationProvider>();
        container.AddSingleton(BuildDestinationProvider);
        container.AddSingleton(BuildResourceRepository);
        container.AddSingleton(BuildDownloadStreamProvider);
        container.AddSingleton(BuildAsyncPolicy);
    }

    private static Job GetJob(IServiceProvider sp)
    {
        return sp.GetRequiredService<EntityRegistry<Job>>().RegisteredEntity ?? throw new Exception("The job is not registered yet.");
    }

    private static ILinkCalculator BuildLinkCalculator(IServiceProvider sp)
    {
        return sp.GetRequiredService<Job>().FullScan
            ? sp.GetRequiredService<FullScanLinkCalculator>()
            : sp.GetRequiredService<LinkCalculator>();
    }

    private static IAsyncPolicy BuildAsyncPolicy(IServiceProvider sp)
    {
        var job = sp.GetRequiredService<Job>();
        var cacheProvider = sp.GetRequiredService<IAsyncCacheProvider>();
        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
        int httpRequestRetries = job.HttpRequestRetries;
        TimeSpan httpDelay = job.HttpRequestDelayBetweenRetries;
        return Singleton.Get(() =>
        {
            var cacheLogger = loggerFactory.CreateLogger("Cache");
            var cachePolicy = Policy.CacheAsync(cacheProvider, DefaultCacheTtl, (_, key) => { cacheLogger.LogRequest("CACHED", key); }, (_, _) => { }, (_, _) => { }, (_, _, _) => { }, (_, _, _) => { });
            var retryPolicy = Policy.Handle<Exception>(ex => ex is not HttpRequestException { StatusCode: >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError })
                .WaitAndRetryAsync(httpRequestRetries, _ => TimeSpan.Zero, (exception, _) => { Console.WriteLine(exception.Message); });

            return Policy.WrapAsync(cachePolicy, retryPolicy, AsyncDelayPolicy.Create(httpDelay));
        });
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
        return resourceRepoArgs switch
        {
            FileSystemResourceRepositoryConfiguration => sp.GetRequiredService<FileSystemResourceRepository>(),
            _ => throw new ArgumentException(
                $"Unknown resource processor config type: {resourceRepoArgs.GetType().Name}",
                nameof(resourceRepoArgs))
        };
    }

    private static IDestinationProvider BuildDestinationProvider(IServiceProvider sp)
    {
        IResourceRepositoryConfiguration jobResourceRepoArgs = GetJobResourceRepoArgs(sp);

        return jobResourceRepoArgs switch
        {
            FileSystemResourceRepositoryConfiguration => sp.GetRequiredService<CompiledDestinationProvider>(),
            _ => throw new ArgumentException(
                $"Unknown resource processor config type: {jobResourceRepoArgs.GetType().Name}",
                nameof(jobResourceRepoArgs))
        };
    }

    private static IResourceRepositoryConfigurationValidator BuildResourceRepositoryConfigurationValidator(
        IServiceProvider sp)
    {
        var jobResourceRepoArgs = GetJobResourceRepoArgs(sp);

        return BuildResourceRepositoryConfigurationValidator(sp, jobResourceRepoArgs);
    }

    private static IResourceRepositoryConfigurationValidator BuildResourceRepositoryConfigurationValidator(
        IServiceProvider sp, IResourceRepositoryConfiguration jobResourceRepoArgs) =>
        jobResourceRepoArgs switch
        {
            FileSystemResourceRepositoryConfiguration => sp
                .GetRequiredService<FileSystemResourceRepositoryConfigurationValidator>(),
            _ => throw new ArgumentException(
                $"Unknown resource processor config type: {jobResourceRepoArgs.GetType().Name}",
                nameof(jobResourceRepoArgs))
        };

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

    private static IResourceRepositoryConfiguration GetJobResourceRepoArgs(IServiceProvider sp)
    {
        var jobRegistry = sp.GetService<EntityRegistry<Job>>();
        var jobDefinitionRegistry = sp.GetService<EntityRegistry<JobDefinition>>();

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
