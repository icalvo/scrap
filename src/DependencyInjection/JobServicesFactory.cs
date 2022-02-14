using System.Net;
using LiteDB;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using Scrap.Downloads;
using Scrap.Jobs;
using Scrap.Pages;
using Scrap.Pages.LiteDb;
using Scrap.Resources;
using Scrap.Resources.FileSystem;

namespace Scrap.DependencyInjection;

public class JobServicesFactory : IJobServicesFactory
{
    private static readonly TimeSpan DefaultCacheTtl = TimeSpan.FromMinutes(5);
    private readonly ILoggerFactory _loggerFactory;
    private readonly IAsyncCacheProvider _cacheProvider;
    private readonly ILiteDatabase _db;
    private readonly ILogger _logger;

    public JobServicesFactory(ILoggerFactory loggerFactory, IConfiguration config)
    {
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<ServicesResolver>();
        _cacheProvider = new MemoryCacheProvider(
            new MemoryCache(new MemoryCacheOptions(), _loggerFactory));
        _db = new LiteDatabase(new ConnectionString(config["Scrap:Database"]));
        _logger.LogDebug("Scrap DB: {ConnectionString}", config["Scrap:Database"]);
    }

    public IPageRetriever GetHttpPageRetriever(Job job)
    {
        return Singleton.Get(() =>
        {
            IAsyncPolicy httpPolicy = BuildHttpPolicy(job);
            var downloadStreamProvider = GetDownloadStreamProvider(job);
            return new HttpPageRetriever(
                downloadStreamProvider,
                httpPolicy,
                _loggerFactory.CreateLogger<HttpPageRetriever>(),
                _loggerFactory);
        });
    }

    public IPageMarkerRepository GetPageMarkerRepository(Job job)
    {
        return Singleton.Get(() => new LiteDbPageMarkerRepository(
            _db,
            _loggerFactory.CreateLogger<LiteDbPageMarkerRepository>(),
            disableWrites: job.DisableMarkingVisited));
    }

    public Task<IResourceRepository> GetResourceRepositoryAsync(Job job)
    {
        return Singleton.GetAsync<IResourceRepository>(async () =>
        {
            switch (job.ResourceRepoArgs)
            {
                case FileSystemResourceRepositoryConfiguration config:
                    _logger.LogDebug("Destination root folder: {RootFolder}", config.RootFolder);
                    var destinationProvider = await CompiledDestinationProvider.CreateCompiledAsync(
                        config.PathFragments,
                        new Logger<CompiledDestinationProvider>(_loggerFactory));
                    return new FileSystemResourceRepository(
                        destinationProvider,
                        config.RootFolder,
                        new Logger<FileSystemResourceRepository>(_loggerFactory),
                        disableWrites: job.DisableResourceWrites);
                default:
                    throw new ArgumentException(
                        $"Unknown resource processor config type: {job.ResourceRepoArgs.GetType().Name}",
                        nameof(job.ResourceRepoArgs));
            }
        });
    }

    public IDownloadStreamProvider GetDownloadStreamProvider(Job job)
    {
        var protocol = "http";
        return Singleton.Get(() =>
        {
            switch (protocol)
            {
                case "http":
                case "https":
                    var httpClient = new HttpClient(
                        new PollyMessageHandler(BuildHttpPolicy(job),
                            _loggerFactory.CreateLogger<HttpClientDownloadStreamProvider>()));
                    return new HttpClientDownloadStreamProvider(httpClient);
                default:
                    throw new ArgumentException($"Unknown URI protocol {protocol}", nameof(protocol));
            }
        });
    }

    public ILinkCalculator GetLinkCalculator(Job job)
    {
        if (job.FullScan)
        {
            return new FullScanLinkCalculator(_loggerFactory.CreateLogger<FullScanLinkCalculator>());
        }

        return new LinkCalculator(_loggerFactory.CreateLogger<LinkCalculator>(), GetPageMarkerRepository(job));
    }

    private IAsyncPolicy BuildHttpPolicy(Job job)
    {
        int httpRequestRetries = job.HttpRequestRetries;
        TimeSpan httpDelay = job.HttpRequestDelayBetweenRetries;
        return Singleton.Get(() =>
        {
            var cacheLogger = _loggerFactory.CreateLogger("Cache");
            var cachePolicy = Policy.CacheAsync(
                _cacheProvider,
                DefaultCacheTtl,
                (_, key) => { cacheLogger.LogRequest("CACHED", key); },
                (_, _) => { },
                (_, _) => { },
                (_, _, _) => { },
                (_, _, _) => { });
            var retryPolicy = Policy
                .Handle<Exception>(ex => ex is not HttpRequestException
                {
                    StatusCode: >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError
                })
                .WaitAndRetryAsync(
                    httpRequestRetries,
                    _ => TimeSpan.Zero,
                    (exception, _) =>
                    {
                        Console.WriteLine(exception.Message);
                    });

            return Policy.WrapAsync(
                cachePolicy,
                retryPolicy,
                AsyncDelayPolicy.Create(httpDelay));
        });
    }

    private class PollyMessageHandler: DelegatingHandler
    {
        private readonly IAsyncPolicy _policy;
        private readonly ILogger _logger;

        public PollyMessageHandler(IAsyncPolicy policy, ILogger logger)
        {
            _policy = policy;
            _logger = logger;
            InnerHandler = new HttpClientHandler();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.LogRequest(request.Method.ToString(), request.RequestUri?.AbsoluteUri);
            return _policy.ExecuteAsync(_ => base.SendAsync(request, cancellationToken), new Context(request.RequestUri?.AbsoluteUri));
        }
    }
}
