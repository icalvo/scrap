using Scrap.Domain;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;

namespace Scrap.Application;

public class ResourcesApplicationService : IResourcesApplicationService
{
    private readonly IJobFactory _jobFactory;
    private readonly IPageRetriever _pageRetriever;

    public ResourcesApplicationService(
        IJobFactory jobFactory, IPageRetriever pageRetriever)
    {
        _jobFactory = jobFactory;
        _pageRetriever = pageRetriever;
    }

    public async IAsyncEnumerable<string> GetResourcesAsync(NewJobDto jobDto, Uri pageUrl, int pageIndex)
    {
        if (jobDto.ResourceType != ResourceType.DownloadLink)
        {
            throw new Exception();
        }

        var job = await _jobFactory.CreateAsync(jobDto);

        var resourceXPath = job.ResourceXPath;

        IEnumerable<ResourceInfo> GetResourceLinks(IPage page, int crawlPageIndex)
            => ResourceLinks(page, crawlPageIndex, resourceXPath);

        var page = await _pageRetriever.GetPageAsync(pageUrl);
        var resources = GetResourceLinks(page, pageIndex)
            .Select(x => x.ResourceUrl.AbsoluteUri);

        foreach (var resource in resources)
        {
            yield return resource;
        }
    }

    private static IEnumerable<ResourceInfo> ResourceLinks(
        IPage page, int crawlPageIndex, XPath resourceXPathExpression)
    {
        var links = page.Links(resourceXPathExpression).ToArray();
        return links.Select((resourceUrl, resourceIndex) => new ResourceInfo(page, crawlPageIndex, resourceUrl, resourceIndex));
    }
}