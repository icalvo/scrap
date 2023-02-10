using Microsoft.Extensions.Logging;
using Polly;
using Scrap.Domain;
using Scrap.Domain.Downloads;
using Scrap.Domain.Jobs;
using Scrap.Domain.Jobs.Graphs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;

namespace Scrap.Application.Scrap;

public class ScrapDownloadsService : IScrapDownloadsService
{
    private const int RetryCount = 5;
    private readonly IFactory<Job, IDownloadStreamProvider> _downloadStreamProviderFactory;
    private readonly IGraphSearch _graphSearch;

    private readonly IAsyncFactory<JobDto, Job> _jobFactory;

    private readonly IFactory<Job, ILinkCalculator> _linkCalculatorFactory;
    private readonly ILogger<ScrapDownloadsService> _logger;
    private readonly IFactory<Job, IPageMarkerRepository> _pageMarkerRepositoryFactory;
    private readonly IFactory<Job, IPageRetriever> _pageRetrieverFactory;
    private readonly IFactory<Job, IResourceRepository> _resourceRepositoryFactory;

    public ScrapDownloadsService(
        IGraphSearch graphSearch,
        ILogger<ScrapDownloadsService> logger,
        IFactory<Job, IDownloadStreamProvider> downloadStreamProviderFactory,
        IFactory<Job, IResourceRepository> resourceRepositoryFactory,
        IFactory<Job, IPageRetriever> pageRetrieverFactory,
        IFactory<Job, IPageMarkerRepository> pageMarkerRepositoryFactory,
        IFactory<Job, ILinkCalculator> linkCalculatorFactory,
        IAsyncFactory<JobDto, Job> jobFactory)
    {
        _graphSearch = graphSearch;
        _logger = logger;
        _resourceRepositoryFactory = resourceRepositoryFactory;
        _pageRetrieverFactory = pageRetrieverFactory;
        _pageMarkerRepositoryFactory = pageMarkerRepositoryFactory;
        _linkCalculatorFactory = linkCalculatorFactory;
        _jobFactory = jobFactory;
        _downloadStreamProviderFactory = downloadStreamProviderFactory;
    }

    public async Task DownloadLinksAsync(JobDto jobDto)
    {
        var job = await _jobFactory.Build(jobDto);
        var resourceRepository = _resourceRepositoryFactory.Build(job);
        var rootUri = job.RootUrl;
        var adjacencyXPath = job.AdjacencyXPath;
        var (resourceXPath, _) = job.GetResourceCapabilitiesOrThrow();

        async Task Download((ResourceInfo info, Stream stream) x)
        {
            var (info, stream) = x;
            await resourceRepository.UpsertAsync(info, stream);
            _logger.LogInformation("Downloaded {Url} to {Key}", info.ResourceUrl,
                await resourceRepository.GetKeyAsync(info));
        }

        IEnumerable<ResourceInfo> GetResourceLinks(IPage page, int crawlPageIndex) =>
            ResourceLinks(page, crawlPageIndex, resourceXPath);

        ValueTask<bool> IsNotDownloaded(ResourceInfo info) =>
            IsNotDownloadedAsync(info, resourceRepository, job.DownloadAlways);

        var pageRetriever = _pageRetrieverFactory.Build(job);
        var linkCalculator = _linkCalculatorFactory.Build(job);
        var pageMarkerRepository = _pageMarkerRepositoryFactory.Build(job);
        var downloadStreamProvider = _downloadStreamProviderFactory.Build(job);
        var pipeline =
            Pages(rootUri, pageRetriever, adjacencyXPath, linkCalculator)
                .Do(page => _logger.LogDebug("Processing page {PageUrl}", page.Uri))
                .DoAwait((page, pageIndex) =>
                {
                    var privatePage = page;
                    return Policy.Handle<Exception>()
                        .RetryAsync(
                            RetryCount,
                            async (ex, retryCount) =>
                            {
                                _logger.LogWarning(
                                    "Error #{RetryCount} processing page resources: {Message}. Reloading page and trying again...",
                                    retryCount, ex.Message);
                                privatePage = await privatePage.RecreateAsync();
                            })
                        .ExecuteAsync(() => GetResourceLinks(privatePage, pageIndex)
                            .ToAsyncEnumerable()
                            .WhereAwait(IsNotDownloaded)
                            .SelectAwait(async resourceLink => (
                                x: resourceLink,
                                stream: await downloadStreamProvider.GetStreamAsync(resourceLink.ResourceUrl)))
                            .DoAwait(Download)
                            .ExecuteAsync());
                })
                .DoAwait(page => pageMarkerRepository.UpsertAsync(page.Uri));

        await pipeline.ExecuteAsync();
    }

    private static IEnumerable<ResourceInfo> ResourceLinks(
        IPage page,
        int crawlPageIndex,
        XPath resourceXPathExpression)
    {
        var links = page.Links(resourceXPathExpression).ToArray();
        return links.Select((resourceUrl, resourceIndex) =>
            new ResourceInfo(page, crawlPageIndex, resourceUrl, resourceIndex));
    }

    private IAsyncEnumerable<IPage> Pages(
        Uri rootUri,
        IPageRetriever pageRetriever,
        XPath? adjacencyXPath,
        ILinkCalculator linkCalculator) =>
        _graphSearch.SearchAsync(
            rootUri,
            pageRetriever.GetPageAsync,
            page => linkCalculator.CalculateLinks(page, adjacencyXPath));

    private async ValueTask<bool> IsNotDownloadedAsync(
        ResourceInfo info,
        IResourceRepository resourceRepository,
        bool downloadAlways)
    {
        if (downloadAlways)
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
}
