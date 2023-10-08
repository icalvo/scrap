using Scrap.Application.Download;
using Scrap.Common;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;

namespace Scrap.Application.Resources;

public class ResourcesApplicationService : IResourcesApplicationService
{
    private readonly IPageRetrieverFactory _pageRetrieverFactory;
    private readonly ICommandJobBuilder<IResourceCommand, IResourcesJob> _siteFactory;

    public ResourcesApplicationService(
        IPageRetrieverFactory pageRetrieverFactory,
        ICommandJobBuilder<IResourceCommand, IResourcesJob> siteFactory)
    {
        _pageRetrieverFactory = pageRetrieverFactory;
        _siteFactory = siteFactory;
    }

    public IAsyncEnumerable<string> GetResourcesAsync(IResourceCommand command) =>
        _siteFactory.Build(command)
            .MapAsync(x => GetResourcesAsync(x.Item1, command.PageUrl, command.PageIndex));

    private async IAsyncEnumerable<string> GetResourcesAsync(IResourcesJob job, Uri pageUrl, int pageIndex)
    {
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
