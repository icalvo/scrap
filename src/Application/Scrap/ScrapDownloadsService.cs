using Microsoft.Extensions.Logging;
using Scrap.Domain;
using Scrap.Domain.Downloads;
using Scrap.Domain.Jobs;
using Scrap.Domain.Jobs.Graphs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;

namespace Scrap.Application.Scrap;

public class ScrapDownloadsService : IScrapDownloadsService
{
    private readonly IGraphSearch _graphSearch;
    private readonly ILogger<ScrapDownloadsService> _logger;
    private readonly IDownloadStreamProvider _downloadStreamProvider;
    private readonly IResourceRepository _resourceRepository;
    private readonly IPageRetriever _pageRetriever;
    private readonly IPageMarkerRepository _pageMarkerRepository;
    private readonly ILinkCalculator _linkCalculator;
    private readonly IJobFactory _jobFactory;

    public ScrapDownloadsService(
        IGraphSearch graphSearch,
        ILogger<ScrapDownloadsService> logger,
        IDownloadStreamProvider downloadStreamProvider,
        IResourceRepository resourceRepository,
        IPageRetriever pageRetriever,
        IPageMarkerRepository pageMarkerRepository,
        ILinkCalculator linkCalculator,
        IJobFactory jobFactory)
    {
        _graphSearch = graphSearch;
        _logger = logger;
        _downloadStreamProvider = downloadStreamProvider;
        _resourceRepository = resourceRepository;
        _pageRetriever = pageRetriever;
        _pageMarkerRepository = pageMarkerRepository;
        _linkCalculator = linkCalculator;
        _jobFactory = jobFactory;
    }

    public async Task DownloadLinksAsync(JobDto jobDto)
    {
        var job = await _jobFactory.CreateAsync(jobDto);
        var rootUri = job.RootUrl;
        var adjacencyXPath = job.AdjacencyXPath;
        var resourceXPath = job.ResourceXPath;

        async Task Download((ResourceInfo info, Stream stream) x)
        {
            var (info, stream) = x;
            await _resourceRepository.UpsertAsync(info, stream);
            _logger.LogInformation("Downloaded {Url} to {Key}", info.ResourceUrl, await _resourceRepository.GetKeyAsync(info));
        }

        IEnumerable<ResourceInfo> GetResourceLinks(IPage page, int crawlPageIndex)
            => ResourceLinks(page, crawlPageIndex, resourceXPath);

        ValueTask<bool> IsNotDownloaded(ResourceInfo info)
            => this.IsNotDownloadedAsync(info, _resourceRepository, job.DownloadAlways);

        var pipeline =
            Pages(rootUri, _pageRetriever, adjacencyXPath, _linkCalculator)
                .Do(page => _logger.LogDebug("Processing page {PageUrl}", page.Uri))
                .DoAwait((page, pageIndex) =>
                    GetResourceLinks(page, pageIndex)
                        .ToAsyncEnumerable()
                        .WhereAwait(IsNotDownloaded)
                        .SelectAwait(async resourceLink => (
                            x: resourceLink,
                            stream: await _downloadStreamProvider.GetStreamAsync(resourceLink.ResourceUrl)))
                        .DoAwait(Download))
                .DoAwait(page => _pageMarkerRepository.UpsertAsync(page.Uri));

        await pipeline.ExecuteAsync();
    }

    private static IEnumerable<ResourceInfo> ResourceLinks(
        IPage page, int crawlPageIndex, XPath resourceXPathExpression)
    {
        var links = page.Links(resourceXPathExpression).ToArray();
        return links.Select((resourceUrl, resourceIndex) => new ResourceInfo(page, crawlPageIndex, resourceUrl, resourceIndex));
    }

    private IAsyncEnumerable<IPage> Pages(
        Uri rootUri,
        IPageRetriever pageRetriever,
        XPath? adjacencyXPath,
        ILinkCalculator linkCalculator)
    {
        return _graphSearch.SearchAsync(
            rootUri,
            pageRetriever.GetPageAsync,
            page => linkCalculator.CalculateLinks(page, adjacencyXPath));
    }

    private async ValueTask<bool> IsNotDownloadedAsync(ResourceInfo info, IResourceRepository resourceRepository, bool downloadAlways)
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
