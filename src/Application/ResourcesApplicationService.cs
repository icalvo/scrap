using Scrap.Domain;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;

namespace Scrap.Application;

public class ResourcesApplicationService : IResourcesApplicationService
{
    private readonly IJobFactory _jobFactory;
    private readonly IPageRetrieverFactory _pageRetrieverFactory;

    public ResourcesApplicationService(
        IJobFactory jobFactory,
        IPageRetrieverFactory pageRetrieverFactory)
    {
        _jobFactory = jobFactory;
        _pageRetrieverFactory = pageRetrieverFactory;
    }

    public async IAsyncEnumerable<string> GetResourcesAsync(JobDto jobDto, Uri pageUrl, int pageIndex)
    {
        if (jobDto.ResourceType != ResourceType.DownloadLink)
        {
            throw new ArgumentException("Job resource type must be DownloadLink", nameof(jobDto));
        }

        var job = await _jobFactory.BuildAsync(jobDto);
        job.ValidateResourceCapabilities();

        var pageRetriever = _pageRetrieverFactory.Build(job);
        var page = await pageRetriever.GetPageAsync(pageUrl);
        var resources = ResourceLinks(page, pageIndex, job.ResourceXPath).Select(x => x.ResourceUrl.AbsoluteUri);

        foreach (var resource in resources)
        {
            yield return resource;
        }
    }

    private static IEnumerable<ResourceInfo> ResourceLinks(
        IPage page,
        int crawlPageIndex,
        XPath resourceXPathExpression)
    {
        var links = page.Links(resourceXPathExpression).ToArray();
        return links.Select(
            (resourceUrl, resourceIndex) => new ResourceInfo(page, crawlPageIndex, resourceUrl, resourceIndex));
    }
}
