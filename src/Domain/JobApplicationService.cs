using System.Text;
using Microsoft.Extensions.Logging;
using Scrap.JobDefinitions;
using Scrap.Jobs;
using Scrap.Jobs.Graphs;
using Scrap.Pages;
using Scrap.Resources;

namespace Scrap;

public class JobApplicationService
{
    private readonly IGraphSearch _graphSearch;
    private readonly ILogger<JobApplicationService> _logger;
    private readonly IJobServicesFactory _servicesFactory;
    private readonly IJobFactory _jobFactory;

    public JobApplicationService(
        IGraphSearch graphSearch,
        IJobServicesFactory servicesFactory,
        IJobFactory jobFactory,
        ILogger<JobApplicationService> logger)
    {
        _graphSearch = graphSearch;
        _logger = logger;
        _servicesFactory = servicesFactory;
        _jobFactory = jobFactory;
    }

    public async Task ScrapAsync(NewJobDto jobDto)
    {
        await (jobDto.ResourceType switch
        {
            ResourceType.DownloadLink => DownloadLinksAsync(jobDto),
            ResourceType.Text => ScrapTextAsync(jobDto),
            _ => throw new Exception($"Invalid resource type")
        });
    }

    public async IAsyncEnumerable<string> TraverseAsync(NewJobDto jobDto)
    {
        var job = await _jobFactory.CreateAsync(jobDto);

        var rootUri = job.RootUrl;
        var adjacencyXPath = job.AdjacencyXPath;
        var pageRetriever = _servicesFactory.GetHttpPageRetriever(job);
        var linkCalculator = _servicesFactory.GetLinkCalculator(job);
        
        await foreach (var page in Pages(rootUri, pageRetriever, adjacencyXPath, linkCalculator)
                           .Select(x => x.Uri.AbsoluteUri))
        {
            yield return page;
        }
    }

    public async IAsyncEnumerable<string> GetResourcesAsync(NewJobDto jobDto, Uri pageUrl, int pageIndex)
    {
        if (jobDto.ResourceType != ResourceType.DownloadLink)
        {
            throw new Exception();
        }

        var job = await _jobFactory.CreateAsync(jobDto);
        var resourceXPath = job.ResourceXPath;
        var pageRetriever = _servicesFactory.GetHttpPageRetriever(job);

        IEnumerable<ResourceInfo> GetResourceLinks(IPage page, int crawlPageIndex)
            => ResourceLinks(page, crawlPageIndex, resourceXPath);

        var page = await pageRetriever.GetPageAsync(pageUrl);
        var resources = GetResourceLinks(page, pageIndex)
            .Select(x => x.ResourceUrl.AbsoluteUri);

        foreach (var resource in resources)
        {
            yield return resource;
        }
    }

    public async Task DownloadAsync(NewJobDto jobDto, Uri pageUrl, int pageIndex, Uri resourceUrl, int resourceIndex)
    {
        if (jobDto.ResourceType != ResourceType.DownloadLink)
        {
            throw new Exception();
        }

        var job = await _jobFactory.CreateAsync(jobDto);
        var downloadStreamProvider = _servicesFactory.GetDownloadStreamProvider(job);
        var resourceRepository = await _servicesFactory.GetResourceRepositoryAsync(job);
        var pageRetriever = _servicesFactory.GetHttpPageRetriever(job);
        
        var page = await pageRetriever.GetPageAsync(pageUrl);

        var info = new ResourceInfo(page, pageIndex, resourceUrl, resourceIndex);
        if (await this.IsNotDownloadedAsync(info, resourceRepository, job.DownloadAlways))
        {
            var stream = await downloadStreamProvider.GetStreamAsync(resourceUrl);
            await resourceRepository.UpsertAsync(info, stream);
            _logger.LogInformation("Downloaded {Url} to {Key}", info.ResourceUrl, await resourceRepository.GetKeyAsync(info));
        }
    }

    public async Task MarkVisitedPageAsync(NewJobDto jobDto, Uri pageUrl)
    {
        var job = await _jobFactory.CreateAsync(jobDto);
        var pageMarkerRepository = _servicesFactory.GetPageMarkerRepository(job);
        
        await pageMarkerRepository.UpsertAsync(pageUrl);
    }

    private async Task DownloadLinksAsync(NewJobDto jobDto)
    {
        var job = await _jobFactory.CreateAsync(jobDto);
        var rootUri = job.RootUrl;
        var adjacencyXPath = job.AdjacencyXPath;
        var resourceXPath = job.ResourceXPath;
        var downloadStreamProvider = _servicesFactory.GetDownloadStreamProvider(job);
        var resourceRepository = await _servicesFactory.GetResourceRepositoryAsync(job);
        var pageRetriever = _servicesFactory.GetHttpPageRetriever(job);
        var pageMarkerRepository = _servicesFactory.GetPageMarkerRepository(job);
        var linkCalculator = _servicesFactory.GetLinkCalculator(job);

        async Task Download((ResourceInfo info, Stream stream) x)
        {
            var (info, stream) = x;
            await resourceRepository.UpsertAsync(info, stream);
            _logger.LogInformation("Downloaded {Url} to {Key}", info.ResourceUrl, await resourceRepository.GetKeyAsync(info));
        }

        IEnumerable<ResourceInfo> GetResourceLinks(IPage page, int crawlPageIndex)
            => ResourceLinks(page, crawlPageIndex, resourceXPath);

        ValueTask<bool> IsNotDownloaded(ResourceInfo info)
            => this.IsNotDownloadedAsync(info, resourceRepository, job.DownloadAlways);

        var pipeline =
            Pages(rootUri, pageRetriever, adjacencyXPath, linkCalculator)
                .Do(page => _logger.LogDebug("Processing page {PageUrl}", page.Uri))
                .DoAwait((page, pageIndex) =>
                    GetResourceLinks(page, pageIndex)
                        .ToAsyncEnumerable()
                        .WhereAwait(IsNotDownloaded)
                        .SelectAwait(async resourceLink => (
                            x: resourceLink,
                            stream: await downloadStreamProvider.GetStreamAsync(resourceLink.ResourceUrl)))
                        .DoAwait(Download))
                .DoAwait(page => pageMarkerRepository.UpsertAsync(page.Uri));

        await pipeline.ExecuteAsync();
    }

    private async Task ScrapTextAsync(NewJobDto jobDto)
    {
        var job = await _jobFactory.CreateAsync(jobDto);
        var rootUri = job.RootUrl;
        var adjacencyXPath = job.AdjacencyXPath;
        var resourceXPath = job.ResourceXPath;
        var resourceRepository = await _servicesFactory.GetResourceRepositoryAsync(job);
        var pageRetriever = _servicesFactory.GetHttpPageRetriever(job);
        var pageMarkerRepository = _servicesFactory.GetPageMarkerRepository(job);
        var linkCalculator = _servicesFactory.GetLinkCalculator(job);

        IAsyncEnumerable<(ResourceInfo info, string text)> PageTexts(IPage page, int crawlPageIndex) =>
            page.Contents(resourceXPath)
                .Where(text => text != null)
                .Select((text, textIndex) => (
                    info: new ResourceInfo(page, crawlPageIndex, page.Uri, textIndex),
                    text: text ?? ""))
                .ToAsyncEnumerable();

        _logger.LogDebug("Defining pipeline...");
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
                        await resourceRepository.GetKeyAsync(x.info))))
            .DoAwait(page => pageMarkerRepository.UpsertAsync(page.Uri));

        await pipeline.ExecuteAsync();
        _logger.LogInformation("Finished!");
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
