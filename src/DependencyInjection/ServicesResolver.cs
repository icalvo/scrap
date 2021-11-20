using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using Scrap.Downloads;
using Scrap.JobDefinitions;
using Scrap.JobDefinitions.JsonFile;
using Scrap.Jobs;
using Scrap.Jobs.Graphs;
using Scrap.Pages;
using Scrap.Pages.LiteDb;
using Scrap.Resources;
using Scrap.Resources.FileSystem;

namespace Scrap.DependencyInjection
{
    public class ServicesResolver : IJobServicesResolver
    {
        private const int DefaultHttpRequestRetries = 5;

        private readonly ILoggerFactory _loggerFactory;
        private static readonly TimeSpan DefaultHttpRequestDelayBetweenRetries = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan DefaultCacheTtl = TimeSpan.FromMinutes(5);
        private readonly IAsyncCacheProvider _cacheProvider;
        private readonly ILiteDatabase _db;
        private readonly IConfiguration _config;

        public ServicesResolver(ILoggerFactory loggerFactory, IConfiguration config)
        {
            
            _loggerFactory = loggerFactory;
            _config = config;
            _cacheProvider = BuildMemoryCacheProvider(_loggerFactory);
            _db = new LiteDatabase(new ConnectionString(_config["Scrap:Database"]));
        }

        public async Task<JobDefinitionsApplicationService> BuildJobDefinitionsApplicationServiceAsync()
        {
            return
                new JobDefinitionsApplicationService(
                    await MemoryJobDefinitionRepository.FromJsonFileAsync(_config["Scrap:Definitions"]),
                    new Logger<JobDefinitionsApplicationService>(_loggerFactory),
                    _loggerFactory);
        }

        public JobApplicationService BuildScrapperApplicationService()
        {
            return new JobApplicationService(
                new DepthFirstGraphSearch(),
                this,
                new JobFactory(_loggerFactory),
                new Logger<JobApplicationService>(_loggerFactory));
        }

        private static MemoryCacheProvider BuildMemoryCacheProvider(ILoggerFactory loggerFactory)
        {
            var cacheProvider = new MemoryCacheProvider(
                new MemoryCache(new MemoryCacheOptions(), loggerFactory));
            return cacheProvider;
        }

        public (IDownloadStreamProvider downloadStreamProvider, IResourceRepository resourceRepository, LinkedPagesCalculator adjacencyCalculator, IPageRetriever pageRetriever) Build(Job job)
        {
            IAsyncPolicy httpPolicy = BuildHttpPolicy(
                job.HttpRequestRetries,
                job.HttpRequestDelayBetweenRetries);
            var downloadStreamProvider = BuildDownloadStreamProvider("http", httpPolicy);
            var pageRetriever = new HttpPageRetriever(
                downloadStreamProvider,
                new Logger<HttpPageRetriever>(_loggerFactory),
                _loggerFactory);
            var resourceRepository = BuildResourceRepository(job.ResourceRepoArgs);
            var pageMarkerRepository = BuildPageMarkerRepository(job.FullScan);
            var adjacencyCalculator = new LinkedPagesCalculator(pageMarkerRepository, new Logger<LinkedPagesCalculator>(_loggerFactory));

            return (downloadStreamProvider, resourceRepository, adjacencyCalculator, pageRetriever);
        }
        
        public IAsyncPolicy BuildHttpPolicy(
            int? httpRequestRetries,
            TimeSpan? httpDelay)
        {
            var cacheLogger = _loggerFactory.CreateLogger("Cache");
            var cachePolicy = Policy.CacheAsync(
                _cacheProvider,
                DefaultCacheTtl,
                (_, key) => { cacheLogger.LogDebug("CACHED {Uri}", key); },
                (_, _) => {  },
                (_, _) => {  },
                (_, _, _) => {  },
                (_, _, _) => {  });
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    httpRequestRetries ?? DefaultHttpRequestRetries,
                    _ => TimeSpan.Zero,
                    (exception, _) =>
                    {
                        Console.WriteLine(exception.Message);
                    });

            return Policy.WrapAsync(
                cachePolicy,
                retryPolicy,
                new AsyncDelayPolicy(httpDelay ?? DefaultHttpRequestDelayBetweenRetries));

        }

        public IPageMarkerRepository BuildPageMarkerRepository(bool fullScan)
        {
            return new LiteDbPageMarkerRepository(
                _db,
                _loggerFactory.CreateLogger<LiteDbPageMarkerRepository>(),
                disableExists: fullScan);
        }
        
        public IResourceRepository BuildResourceRepository(IResourceRepositoryConfiguration configuration)
        {
            switch (configuration)
            {
                case FileSystemResourceRepositoryConfiguration config:
                    var destinationProvider = CompiledDestinationProvider.CreateCompiled(
                        config.DestinationExpression,
                        new Logger<CompiledDestinationProvider>(_loggerFactory));
                    return new FileSystemResourceRepository(
                        destinationProvider,
                        config.DestinationRootFolder,
                        new Logger<FileSystemResourceRepository>(_loggerFactory));
                default:
                    throw new ArgumentException(
                        "Unknown resource processor config type: " + configuration.GetType().Name,
                        nameof(configuration));
            }
        }

        public IDownloadStreamProvider BuildDownloadStreamProvider(string protocol, IAsyncPolicy policy)
        {
            switch (protocol)
            {
                case "http":
                case "https":
                    var httpClient = new HttpClient(new PollyMessageHandler(policy));
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
                InnerHandler = new HttpClientHandler();
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _policy.ExecuteAsync(_ => base.SendAsync(request, cancellationToken), new Context(request.RequestUri.AbsoluteUri));
            }
        }
    }
}
