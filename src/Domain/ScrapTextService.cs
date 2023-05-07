using System.Text;
using Microsoft.Extensions.Logging;
using Scrap.Common;
using Scrap.Common.Graphs;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;

namespace Scrap.Domain;

public class ScrapTextService : IScrapTextService
{
    private readonly IGraphSearch _graphSearch;
    private readonly ILinkCalculatorFactory _linkCalculatorFactory;
    private readonly IVisitedPageRepositoryFactory _visitedPageRepositoryFactory;
    private readonly IPageRetrieverFactory _pageRetrieverFactory;
    private readonly IResourceRepositoryFactory _resourceRepositoryFactory;
    private readonly ILogger<ScrapTextService> _logger;

    public ScrapTextService(
        IGraphSearch graphSearch,
        IResourceRepositoryFactory resourceRepositoryFactory,
        IPageRetrieverFactory pageRetrieverFactory,
        IVisitedPageRepositoryFactory visitedPageRepositoryFactory,
        ILinkCalculatorFactory linkCalculatorFactory,
        ILogger<ScrapTextService> logger)
    {
        _graphSearch = graphSearch;
        _logger = logger;
        _resourceRepositoryFactory = resourceRepositoryFactory;
        _pageRetrieverFactory = pageRetrieverFactory;
        _visitedPageRepositoryFactory = visitedPageRepositoryFactory;
        _linkCalculatorFactory = linkCalculatorFactory;
    }

    public async Task ScrapTextAsync(Job job)
    {
        if (job.ResourceType != ResourceType.Text)
        {
            throw new InvalidOperationException($"Invalid resource type (should be {nameof(ResourceType.Text)})");
        }

        var resourceRepository = await _resourceRepositoryFactory.BuildAsync(job);
        var rootUri = job.RootUrl;
        var adjacencyXPath = job.AdjacencyXPath;
        job.ValidateResourceCapabilities();

        IAsyncEnumerable<(ResourceInfo info, string text)> PageTexts(IPage page, int crawlPageIndex) =>
            page.Contents(job.ResourceXPath).Where(text => text != null).Select(
                (text, textIndex) => (info: new ResourceInfo(page, crawlPageIndex, page.Uri, textIndex),
                    text: text ?? "")).ToAsyncEnumerable();

        _logger.LogDebug("Defining pipeline...");
        var pageRetriever = _pageRetrieverFactory.Build(job);
        var linkCalculator = _linkCalculatorFactory.Build(job);
        var visitedPageRepository = _visitedPageRepositoryFactory.Build(job);
        var pipeline =
            Pages(rootUri, pageRetriever, adjacencyXPath, linkCalculator)
            .DoAwait((page, pageIndex) =>
                PageTexts(page, pageIndex)
                .WhereAwait(x => IsNotDownloadedAsync(x.info, resourceRepository, job.DownloadAlways))
                .Select(x => (x.info, stream: (Stream)new MemoryStream(Encoding.UTF8.GetBytes(x.text))))
                .DoAwait(y => resourceRepository.UpsertAsync(y.info, y.stream)).DoAwait(
                    async x => _logger.LogInformation(
                        "Downloaded text from {Url} to {Key}",
                        x.info.ResourceUrl,
                        await resourceRepository.GetKeyAsync(x.info))).ExecuteAsync())
            .DoAwait(page => visitedPageRepository.UpsertAsync(page.Uri));

        await pipeline.ExecuteAsync();
        _logger.LogInformation("Finished!");
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
