using Scrap.Domain;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;

namespace Scrap.Application;

public class ResourcesApplicationService : IResourcesApplicationService
{
    private readonly IAsyncFactory<JobDto, Job> _jobFactory;
    private readonly IFactory<Job, IPageRetriever> _pageRetrieverFactory;

    public ResourcesApplicationService(
        IAsyncFactory<JobDto, Job> jobFactory,
        IFactory<Job, IPageRetriever> pageRetrieverFactory)
    {
        _jobFactory = jobFactory;
        _pageRetrieverFactory = pageRetrieverFactory;
    }

    public async IAsyncEnumerable<string> GetResourcesAsync(JobDto jobDto, Uri pageUrl, int pageIndex)
    {
        if (jobDto.ResourceType != ResourceType.DownloadLink)
        {
            throw new Exception();
        }

        var job = await _jobFactory.Build(jobDto);

        var resourceXPath = job.ResourceXPath;

        IEnumerable<ResourceInfo> GetResourceLinks(IPage page, int crawlPageIndex)
            => ResourceLinks(page, crawlPageIndex, resourceXPath);

        var pageRetriever = _pageRetrieverFactory.Build(job);
        var page = await pageRetriever.GetPageAsync(pageUrl);
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
