using System.Text;
using Microsoft.Extensions.Logging;
using Scrap.Downloads;
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
    private readonly IJobServicesResolver _servicesResolver;
    private readonly IJobFactory _jobFactory;

    public JobApplicationService(
        IGraphSearch graphSearch,
        IJobServicesResolver servicesResolver,
        IJobFactory jobFactory,
        ILogger<JobApplicationService> logger)
    {
        _graphSearch = graphSearch;
        _logger = logger;
        _servicesResolver = servicesResolver;
        _jobFactory = jobFactory;
    }

    public async Task ScrapAsync(NewJobDto jobDto)
    {
        _logger.LogTrace("Trace enabled");
        _logger.LogInformation("Starting...");
        await (jobDto.ResourceType switch
        {
            ResourceType.DownloadLink => DownloadLinksAsync(jobDto),
            ResourceType.Text => ScrapTextAsync(jobDto),
            _ => throw new ArgumentOutOfRangeException()
        });
        _logger.LogInformation("Finished!");
    }

    public IAsyncEnumerable<string> TraverseAsync(NewJobDto jobDto)
    {
        _logger.LogTrace("Trace enabled");
        var job = _jobFactory.Create(jobDto);

        var (rootUri, adjacencyXPath, _, _, _, pageRetriever, pageMarkerRepository) =
            GetJobInfoAndCreateDependencies(job);

        return Pages(rootUri, pageRetriever, adjacencyXPath, pageMarkerRepository)
            .Select(x => x.Uri.AbsoluteUri);
    }

    public async IAsyncEnumerable<string> GetResourcesAsync(NewJobDto jobDto, Uri pageUrl, int pageIndex)
    {
        if (jobDto.ResourceType != ResourceType.DownloadLink)
        {
            throw new Exception();
        }

        _logger.LogTrace("Trace enabled");

        var job = _jobFactory.Create(jobDto);
        var (_, _, resourceXPath, _, _, pageRetriever, _) =
            GetJobInfoAndCreateDependencies(job);

        IAsyncEnumerable<ResourceInfo> GetResourceLinks(Page page, int crawlPageIndex)
            => ResourceLinks(page, crawlPageIndex, resourceXPath);

        var page = await pageRetriever.GetPageAsync(pageUrl);
        var resources = GetResourceLinks(page, pageIndex)
            .Select(x => x.ResourceUrl.AbsoluteUri);

        await foreach (var resource in resources)
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

        _logger.LogTrace("Trace enabled");

        var job = _jobFactory.Create(jobDto);
        var (_, _, _, downloadStreamProvider, resourceRepository, pageRetriever, _) =
            GetJobInfoAndCreateDependencies(job);
        
        var page = await pageRetriever.GetPageAsync(pageUrl);

        var info = new ResourceInfo(page, pageIndex, resourceUrl, resourceIndex);
        if (await this.IsNotDownloadedAsync(info, resourceRepository, job.DownloadAlways))
        {
            var stream = await downloadStreamProvider.GetStreamAsync(resourceUrl);
            await resourceRepository.UpsertAsync(info, stream);
            _logger.LogInformation("Downloaded {Url} to {Key}", info.ResourceUrl, await resourceRepository.GetKeyAsync(info));
        }
    }

    public Task MarkVisitedPageAsync(NewJobDto jobDto, Uri pageUrl)
    {
        var job = _jobFactory.Create(jobDto);
        var (_, _, _, _, _, _, pageMarkerRepository) =
            GetJobInfoAndCreateDependencies(job);
        
        return pageMarkerRepository.UpsertAsync(pageUrl);
    }

    private Task DownloadLinksAsync(NewJobDto jobDto)
    {
        var job = _jobFactory.Create(jobDto);
        var (rootUri, adjacencyXPath, resourceXPath, downloadStreamProvider, resourceRepository, pageRetriever, pageMarkerRepository) =
            GetJobInfoAndCreateDependencies(job);

        async Task Download((ResourceInfo info, Stream stream) x)
        {
            var (info, stream) = x;
            await resourceRepository.UpsertAsync(info, stream);
            _logger.LogInformation("Downloaded {Url} to {Key}", info.ResourceUrl, await resourceRepository.GetKeyAsync(info));
        }

        IAsyncEnumerable<ResourceInfo> GetResourceLinks(Page page, int crawlPageIndex)
            => ResourceLinks(page, crawlPageIndex, resourceXPath);

        ValueTask<bool> IsNotDownloaded(ResourceInfo info)
            => this.IsNotDownloadedAsync(info, resourceRepository, job.DownloadAlways);

        var pipeline =
            Pages(rootUri, pageRetriever, adjacencyXPath, pageMarkerRepository)
                .Do(page => _logger.LogInformation("Processing page {PageUrl}", page.Uri))
                .DoAwait((page, pageIndex) =>
                    GetResourceLinks(page, pageIndex)
                        .WhereAwait(IsNotDownloaded)
                        .SelectAwait(async resourceLink => (
                            x: resourceLink,
                            stream: await downloadStreamProvider.GetStreamAsync(resourceLink.ResourceUrl)))
                        .DoAwait(Download))
                .DoAwait(page => pageMarkerRepository.UpsertAsync(page.Uri));

        return pipeline.ExecuteAsync();
    }

    private async Task ScrapTextAsync(NewJobDto jobDto)
    {
        var job = _jobFactory.Create(jobDto);
        var (rootUri, adjacencyXPath,resourceXPath, _, resourceRepository, pageRetriever, pageMarkerRepository) =
            GetJobInfoAndCreateDependencies(job);
            
        IAsyncEnumerable<(ResourceInfo info, string text)> PageTexts(Page page, int crawlPageIndex) =>
            page.Contents(resourceXPath)
                .Where(text => text != null)
                .Select((text, textIndex) => (
                    info: new ResourceInfo(page, crawlPageIndex, page.Uri, textIndex),
                    text: text ?? ""))
                .ToAsyncEnumerable();

        _logger.LogDebug("Defining pipeline...");
        var pipeline =
            Pages(rootUri, pageRetriever, adjacencyXPath, pageMarkerRepository)
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

    private (
        Uri rootUri,
        XPath? adjacencyXPath,
        XPath resourceXPath,
        IDownloadStreamProvider downloadStreamProvider,
        IResourceRepository resourceRepository,
        IPageRetriever pageRetriever,
        IPageMarkerRepository pageMarkerRepository) GetJobInfoAndCreateDependencies(Job job)
    {
        var (rootUri, adjacencyXPath, resourceXPath) =
            (job.RootUrl, job.AdjacencyXPath, job.ResourceXPath);

        job.Log(_logger);

        _logger.LogDebug("Building job-specific dependencies...");
        var (downloadStreamProvider, resourceRepository, pageRetriever, pageMarkerRepository) = _servicesResolver.BuildJobDependencies(job);
            
        return (rootUri, adjacencyXPath, resourceXPath, downloadStreamProvider, resourceRepository, pageRetriever, pageMarkerRepository);
    }

    private static IAsyncEnumerable<ResourceInfo> ResourceLinks(
        Page page, int crawlPageIndex, XPath resourceXPathExpression)
    {
        var links = page.Links(resourceXPathExpression).ToArray();
        return links.Select((resourceUrl, resourceIndex) => new ResourceInfo(page, crawlPageIndex, resourceUrl, resourceIndex))
            .ToAsyncEnumerable();
    }

    private async IAsyncEnumerable<Uri> CalculateLinks(
        Page page,
        XPath? adjacencyXPath,
        IPageMarkerRepository pageMarkerRepository)
    {
        if (adjacencyXPath == null)
        {
            yield break;
        }

        var links = page.Links(adjacencyXPath).ToArray();
        if (links.Length == 0)
        {
            _logger.LogTrace("No links at {PageUri}", page.Uri);
            yield break;
        }

        foreach (var link in links)
        {
            if (await pageMarkerRepository.ExistsAsync(link))
            {
                _logger.LogTrace("Page {Link} already visited", link);
                continue;
            }

            yield return link;
        }
    }
    private IAsyncEnumerable<Page> Pages(
        Uri rootUri,
        IPageRetriever pageRetriever,
        XPath? adjacencyXPath,
        IPageMarkerRepository pageMarkerRepository)
    {
        return _graphSearch.SearchAsync(
            rootUri,
            pageRetriever.GetPageAsync,
            page => CalculateLinks(page, adjacencyXPath, pageMarkerRepository));
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
        _logger.LogInformation("{Resource} already downloaded", key);

        return false;
    }
}
