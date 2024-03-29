﻿using Scrap.Common;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;

namespace Scrap.Application.Resources;

public class ResourcesApplicationService : IResourcesApplicationService
{
    private readonly IPageRetrieverFactory _pageRetrieverFactory;
    private readonly IJobBuilder _sitesApplicationBuilder;

    public ResourcesApplicationService(IPageRetrieverFactory pageRetrieverFactory, IJobBuilder sitesApplicationBuilder)
    {
        _pageRetrieverFactory = pageRetrieverFactory;
        _sitesApplicationBuilder = sitesApplicationBuilder;
    }

    public IAsyncEnumerable<string> GetResourcesAsync(IResourceCommand oneCommand) =>
        _sitesApplicationBuilder
            .BuildJobAsync(oneCommand.NameOrRootUrl, oneCommand.FullScan, oneCommand.DownloadAlways, oneCommand.DisableMarkingVisited, oneCommand.DisableResourceWrites)
            .MapAsync(x => GetResourcesAsync(x.job, oneCommand.PageUrl, oneCommand.PageIndex));

    private async IAsyncEnumerable<string> GetResourcesAsync(Job job, Uri pageUrl, int pageIndex)
    {
        if (job.ResourceType != ResourceType.DownloadLink)
        {
            throw new ArgumentException("Job resource type must be DownloadLink", nameof(job));
        }

        job.ValidateResourceCapabilities();
        var pageRetriever = _pageRetrieverFactory.Build(job);
        var page = await pageRetriever.GetPageAsync(pageUrl);
        var resources = ResourceLinks(page, pageIndex, job.ResourceXPath!).Select(x => x.ResourceUrl.AbsoluteUri);

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
