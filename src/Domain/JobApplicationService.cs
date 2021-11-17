using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Scrap.Downloads;
using Scrap.Graphs;
using Scrap.Jobs;
using Scrap.Pages;
using Scrap.ResourceDownloaders;

namespace Scrap
{
    public class JobApplicationService
    {
        private readonly IGraphSearch _graphSearch;
        private readonly ILogger<JobApplicationService> _logger;
        private readonly PageMarkerRepositoryFactory _pageMarkerRepositoryFactory;
        private readonly HttpPolicyFactory _httpPolicyFactory;
        private readonly DownloadStreamProviderFactory _downloadStreamProviderFactory;
        private readonly ResourceProcessorFactory _resourceProcessorFactory;
        private readonly ILoggerFactory _loggerFactory;

        public JobApplicationService(
            IGraphSearch graphSearch,
            ILogger<JobApplicationService> logger,
            PageMarkerRepositoryFactory pageMarkerRepositoryFactory,
            HttpPolicyFactory httpPolicyFactory,
            DownloadStreamProviderFactory downloadStreamProviderFactory,
            ResourceProcessorFactory resourceProcessorFactory,
            ILoggerFactory loggerFactory)
        {
            _graphSearch = graphSearch;
            _logger = logger;
            _pageMarkerRepositoryFactory = pageMarkerRepositoryFactory;
            _httpPolicyFactory = httpPolicyFactory;
            _downloadStreamProviderFactory = downloadStreamProviderFactory;
            _resourceProcessorFactory = resourceProcessorFactory;
            _loggerFactory = loggerFactory;
        }

        public Task RunAsync(NewJobDto jobDto)
        {
            var job = new Job(jobDto);
            job.ResourceRepoArgs.Validate(_loggerFactory);
            return RunAsync(job);
        }

        public Task RunAsync(JobDto jobDto)
        {
            var job = new Job(jobDto);
            return RunAsync(job);
        }

        private async Task RunAsync(Job job)
        {
            var (rootUrl, adjacencyXPath, adjacencyAttribute, resourceXPath, resourceAttribute, resourceRepoArgs) =
                (job.RootUrl, job.AdjacencyXPath, job.AdjacencyAttribute, job.ResourceXPath, job.ResourceAttribute, job.ResourceRepoArgs);

            job.Log(_logger);
            
            var rootUri = new Uri(rootUrl ?? throw new InvalidOperationException("Root URL must not be null"));
            var baseUrl = new Uri(rootUri.Scheme + "://" + rootUri.Host);

            IAsyncPolicy httpPolicy = _httpPolicyFactory.Build(
                job.HttpRequestRetries,
                job.HttpRequestDelayBetweenRetries);
            IDownloadStreamProvider downloadStreamProvider = _downloadStreamProviderFactory.Build("http", httpPolicy);
            var resourceDownloader = _resourceProcessorFactory.Build(resourceRepoArgs, downloadStreamProvider);
            var pageRetriever = new HttpPageRetriever(
                downloadStreamProvider,
                new Logger<HttpPageRetriever>(_loggerFactory),
                _loggerFactory);
            var pageMarkerRepository = _pageMarkerRepositoryFactory.Build(job.FullScan);
            var adjacencyCalculator = new LinkedPagesCalculator(pageMarkerRepository, new NullLogger<LinkedPagesCalculator>());
            
            var pages = _graphSearch.SearchAsync(
                rootUri,
                uri => pageRetriever.GetPageAsync(uri),
                page => adjacencyCalculator.GetLinkedPagesAsync(page, adjacencyXPath, adjacencyAttribute, baseUrl));

            await foreach (var (page, pageIndex) in pages.Select((p, idx) => (p, idx)))
            {
                var resources = page.Links(resourceXPath, resourceAttribute, baseUrl).ToArray();
                if (!resources.Any())
                {
                    _logger.LogInformation("No resources in this page");
                    continue;
                }

                foreach (var (resourceUrl, resourceIndex) in resources.Select((res, idx) => (res, idx)))
                {
                    await resourceDownloader.DownloadResourceAsync(page, pageIndex, resourceUrl, resourceIndex);
                }
            }
            
            _logger.LogInformation("Finished!");
        }

        private async IAsyncEnumerable<(Page, int, Uri, int)> GetResourcesAsync(Job job)
        {
            var (rootUrl, adjacencyXPath, adjacencyAttribute, resourceXPath, resourceAttribute, resourceRepoArgs) =
                (job.RootUrl, job.AdjacencyXPath, job.AdjacencyAttribute, job.ResourceXPath, job.ResourceAttribute, job.ResourceRepoArgs);

            job.Log(_logger);
            
            var rootUri = new Uri(rootUrl ?? throw new InvalidOperationException("Root URL must not be null"));
            var baseUrl = new Uri(rootUri.Scheme + "://" + rootUri.Host);

            IAsyncPolicy httpPolicy = _httpPolicyFactory.Build(
                job.HttpRequestRetries,
                job.HttpRequestDelayBetweenRetries);
            IDownloadStreamProvider downloadStreamProvider = _downloadStreamProviderFactory.Build("http", httpPolicy);
            var resourceDownloader = _resourceProcessorFactory.Build(resourceRepoArgs, downloadStreamProvider);

            var pageRetriever = new HttpPageRetriever(
                downloadStreamProvider,
                new Logger<HttpPageRetriever>(_loggerFactory),
                _loggerFactory);
            Task<Page> GetPageWithPolicyAsync(Uri uri)
                => ApplyHttpPolicyAsync(httpPolicy, uri, () => pageRetriever.GetPageAsync(uri));

            var pageMarkerRepository = _pageMarkerRepositoryFactory.Build(job.FullScan);
            var adjacencyCalculator = new LinkedPagesCalculator(pageMarkerRepository, new NullLogger<LinkedPagesCalculator>());
            
            IAsyncEnumerable<Uri> AdjacencyFunction(Page page)
                => adjacencyCalculator.GetLinkedPagesAsync(page, adjacencyXPath, adjacencyAttribute, baseUrl);
            
            var pages = _graphSearch.SearchAsync(rootUri, GetPageWithPolicyAsync, AdjacencyFunction);

            await foreach (var (page, pageIndex) in pages.Select((p, idx) => (p, idx)))
            {
                var resources = page.Links(resourceXPath, resourceAttribute, baseUrl).ToArray();
                if (!resources.Any())
                {
                    _logger.LogInformation("No resources in this page");
                    continue;
                }

                foreach (var (resourceUrl, resourceIndex) in resources.Select((res, idx) => (res, idx)))
                {
                    yield return (page, pageIndex, resourceUrl, resourceIndex);
                }
            }
        }

        private static Task<T> ApplyHttpPolicyAsync<T>(IAsyncPolicy httpPolicy, Uri uri, Func<Task<T>> action)
        {
            return httpPolicy.ExecuteAsync(_ => action(), new Context(uri.AbsoluteUri));
        }

        private static Task ApplyHttpPolicyAsync(IAsyncPolicy httpPolicy, Uri uri, Func<Task> action)
        {
            return httpPolicy.ExecuteAsync(_ => action(), new Context(uri.AbsoluteUri));
        }
    }
}