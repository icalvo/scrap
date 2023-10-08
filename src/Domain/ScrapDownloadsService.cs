using Microsoft.Extensions.Logging;
using Polly;
using Scrap.Common;
using Scrap.Common.Graphs;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;

namespace Scrap.Domain;

public class ScrapDownloadsService : IScrapDownloadsService
{
    private const int RetryCount = 5;
    private readonly IDownloadStreamProviderFactory _downloadStreamProviderFactory;
    private readonly IGraphSearch _graphSearch;

    private readonly ILinkCalculatorFactory _linkCalculatorFactory;
    private readonly IVisitedPageRepositoryFactory _visitedPageRepositoryFactory;
    private readonly IPageRetrieverFactory _pageRetrieverFactory;
    private readonly IResourceRepositoryFactory _resourceRepositoryFactory;
    private readonly ILogger<ScrapDownloadsService> _logger;

    public ScrapDownloadsService(
        IGraphSearch graphSearch,
        ILogger<ScrapDownloadsService> logger,
        IDownloadStreamProviderFactory downloadStreamProviderFactory,
        IResourceRepositoryFactory resourceRepositoryFactory,
        IPageRetrieverFactory pageRetrieverFactory,
        IVisitedPageRepositoryFactory visitedPageRepositoryFactory,
        ILinkCalculatorFactory linkCalculatorFactory)
    {
        _graphSearch = graphSearch;
        _logger = logger;
        _resourceRepositoryFactory = resourceRepositoryFactory;
        _pageRetrieverFactory = pageRetrieverFactory;
        _visitedPageRepositoryFactory = visitedPageRepositoryFactory;
        _linkCalculatorFactory = linkCalculatorFactory;
        _downloadStreamProviderFactory = downloadStreamProviderFactory;
    }

    public async Task DownloadLinksAsync(ISingleScrapJob job)
    {
        var resourceRepository = await _resourceRepositoryFactory.BuildAsync(job);
        var rootUri = job.RootUrl;
        var adjacencyXPath = job.AdjacencyXPath;

        var downloadStreamProvider = _downloadStreamProviderFactory.Build(job);

        var pageRetriever = _pageRetrieverFactory.Build(job);
        var linkCalculator = _linkCalculatorFactory.Build(job);
        var visitedPageRepository = _visitedPageRepositoryFactory.Build(job);

        var pipeline = Pages()
            .Do(page => _logger.LogDebug("Processing page {PageUrl}", page.Uri))
            .DoAwait(ProcessPageAsync)
            .DoAwait(page => visitedPageRepository.UpsertAsync(page.Uri));

        await pipeline.ExecuteAsync();
        return;

        IAsyncEnumerable<IPage> Pages() =>
            _graphSearch.SearchAsync(
                rootUri,
                pageRetriever.GetPageAsync,
                page => linkCalculator.CalculateLinks(page, adjacencyXPath));

        Task ProcessPageAsync(IPage page, int pageIndex)
        {
            var privatePage = page;

            return Policy.Handle<Exception>()
                .RetryAsync(
                    RetryCount,
                    async (ex, retryCount) =>
                    {
                        _logger.LogWarning("Error #{RetryCount} processing page resources: {Message}. Reloading page and trying again...", retryCount, ex.Message);
                        privatePage = await privatePage.ReloadAsync();
                    })
                .ExecuteAsync(ProcessResourceAsync);

            Task ProcessResourceAsync() =>
                ResourceLinks().ToAsyncEnumerable()
                    .WhereAwait(IsNotDownloaded)
                    .SelectAwait(async resourceLink =>
                        (x: resourceLink, stream: await downloadStreamProvider.GetStreamAsync(resourceLink.ResourceUrl)))
                    .DoAwait(Download)
                    .ExecuteAsync();

            IEnumerable<ResourceInfo> ResourceLinks()
            {
                var links = page.Links(job.ResourceXPath).ToArray();
                return links.Select(
                    (resourceUrl, resourceIndex) => new ResourceInfo(page, pageIndex, resourceUrl, resourceIndex));
            }

        }

        async ValueTask<bool> IsNotDownloaded(
            ResourceInfo info)
        {
            if (job.DownloadAlways)
            {
                return true;
            }

            var exists = await resourceRepository.ExistsAsync(info);
            if (!exists)
            {
                return true;
            }

            var key = await resourceRepository.GetKeyAsync(info);
            _logger.LogDebug("{Resource} already downloaded", key);

            return false;
        }

        async Task Download((ResourceInfo info, Stream stream) x)
        {
            var (info, stream) = x;
            await resourceRepository.UpsertAsync(info, stream);
            _logger.LogInformation("Downloading {Url}", info.ResourceUrl);
            var key = await resourceRepository.GetKeyAsync(info);
            _logger.LogInformation("Downloaded to {Key}", key);
        }
    }
}
