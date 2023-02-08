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
    private readonly IAsyncFactory<JobDto, Job> _jobFactory;
    private readonly IFactory<Job, ILinkCalculator> _linkCalculatorFactory;
    private readonly ILogger<ScrapTextService> _logger;
    private readonly IFactory<Job, IPageMarkerRepository> _pageMarkerRepositoryFactory;
    private readonly IFactory<Job, IPageRetriever> _pageRetrieverFactory;
    private readonly IFactory<Job, IResourceRepository> _resourceRepositoryFactory;

    public ScrapTextService(
        IGraphSearch graphSearch,
        IAsyncFactory<JobDto, Job> jobFactory,
        IFactory<Job, IResourceRepository> resourceRepositoryFactory,
        IFactory<Job, IPageRetriever> pageRetrieverFactory,
        IFactory<Job, IPageMarkerRepository> pageMarkerRepositoryFactory,
        IFactory<Job, ILinkCalculator> linkCalculatorFactory,
        ILogger<ScrapTextService> logger)
    {
        _graphSearch = graphSearch;
        _logger = logger;
        _jobFactory = jobFactory;
        _resourceRepositoryFactory = resourceRepositoryFactory;
        _pageRetrieverFactory = pageRetrieverFactory;
        _pageMarkerRepositoryFactory = pageMarkerRepositoryFactory;
        _linkCalculatorFactory = linkCalculatorFactory;
    }

    public async Task ScrapTextAsync(JobDto jobDto)
    {
        if (jobDto.ResourceType != ResourceType.Text)
        {
            throw new InvalidOperationException($"Invalid resource type (should be {nameof(ResourceType.Text)})");
        }

        var job = await _jobFactory.Build(jobDto);

        var resourceRepository = _resourceRepositoryFactory.Build(job);
        var rootUri = job.RootUrl;
        var adjacencyXPath = job.AdjacencyXPath;
        var (resourceXPath, _) = job.GetResourceCapabilitiesOrThrow();

        IAsyncEnumerable<(ResourceInfo info, string text)> PageTexts(IPage page, int crawlPageIndex) =>
            page.Contents(resourceXPath)
                .Where(text => text != null)
                .Select((text, textIndex) => (
                    info: new ResourceInfo(page, crawlPageIndex, page.Uri, textIndex),
                    text: text ?? ""))
                .ToAsyncEnumerable();

        _logger.LogDebug("Defining pipeline...");
        var pageRetriever = _pageRetrieverFactory.Build(job);
        var linkCalculator = _linkCalculatorFactory.Build(job);
        var pageMarkerRepository = _pageMarkerRepositoryFactory.Build(job);
        var pipeline =
            Pages(rootUri, pageRetriever, adjacencyXPath, linkCalculator)
                .DoAwait((page, pageIndex) =>
                    PageTexts(page, pageIndex)
                        .WhereAwait(x => IsNotDownloadedAsync(x.info, resourceRepository, job.DownloadAlways))
                        .Select(x => (
                            x.info,
                            stream: (Stream)new MemoryStream(Encoding.UTF8.GetBytes(x.text))))
                        .DoAwait(y => resourceRepository.UpsertAsync(y.info, y.stream))
                        .DoAwait(async x =>
                            _logger.LogInformation(
                                "Downloaded text from {Url} to {Key}",
                                x.info.ResourceUrl,
                                await resourceRepository.GetKeyAsync(x.info)))
                        .ExecuteAsync())
                .DoAwait(page => pageMarkerRepository.UpsertAsync(page.Uri));

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
