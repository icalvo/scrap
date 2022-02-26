using System.Text;
using Microsoft.Extensions.Logging;
using Scrap.Domain;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
using Scrap.Domain.Jobs.Graphs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;

namespace Scrap.Application.Scrap;

public class ScrapTextService : IScrapTextService
{
    private readonly IGraphSearch _graphSearch;
    private readonly ILogger<ScrapTextService> _logger;
    private readonly IJobFactory _jobFactory;
    private readonly IResourceRepository _resourceRepository;
    private readonly IPageRetriever _pageRetriever;
    private readonly IPageMarkerRepository _pageMarkerRepository;
    private readonly ILinkCalculator _linkCalculator;

    public ScrapTextService(
        IGraphSearch graphSearch,
        IJobFactory jobFactory,
        IResourceRepository resourceRepository,
        IPageRetriever pageRetriever,
        IPageMarkerRepository pageMarkerRepository,
        ILinkCalculator linkCalculator,
        ILogger<ScrapTextService> logger)
    {
        _graphSearch = graphSearch;
        _logger = logger;
        _jobFactory = jobFactory;
        _resourceRepository = resourceRepository;
        _pageRetriever = pageRetriever;
        _pageMarkerRepository = pageMarkerRepository;
        _linkCalculator = linkCalculator;
    }

    public async Task ScrapTextAsync(NewJobDto jobDto)
    {
        if (jobDto.ResourceType != ResourceType.Text)
        {
            throw new InvalidOperationException($"Invalid resource type (should be {nameof(ResourceType.Text)})");
        }

        var job = await _jobFactory.CreateAsync(jobDto);

        var rootUri = job.RootUrl;
        var adjacencyXPath = job.AdjacencyXPath;
        var resourceXPath = job.ResourceXPath;

        IAsyncEnumerable<(ResourceInfo info, string text)> PageTexts(IPage page, int crawlPageIndex) =>
            page.Contents(resourceXPath)
                .Where(text => text != null)
                .Select((text, textIndex) => (
                    info: new ResourceInfo(page, crawlPageIndex, page.Uri, textIndex),
                    text: text ?? ""))
                .ToAsyncEnumerable();

        _logger.LogDebug("Defining pipeline...");
        var pipeline =
            Pages(rootUri, _pageRetriever, adjacencyXPath, _linkCalculator)
                .DoAwait((page, pageIndex) =>
                    PageTexts(page, pageIndex)
                        .WhereAwait(x => IsNotDownloadedAsync(x.info, _resourceRepository, job.DownloadAlways))
                        .Select(x => (
                            x.info,
                            stream: (Stream)new MemoryStream(Encoding.UTF8.GetBytes(x.text))))
                        .DoAwait(y => _resourceRepository.UpsertAsync(y.info, y.stream))
                        .DoAwait(async x =>
                            _logger.LogInformation(
                                "Downloaded text from {Url} to {Key}",
                                x.info.ResourceUrl,
                                await _resourceRepository.GetKeyAsync(x.info))))
                .DoAwait(page => _pageMarkerRepository.UpsertAsync(page.Uri));

        await pipeline.ExecuteAsync();
        _logger.LogInformation("Finished!");
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
