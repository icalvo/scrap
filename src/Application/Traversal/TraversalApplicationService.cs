using Scrap.Common;
using Scrap.Common.Graphs;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;

namespace Scrap.Application.Traversal;

public class TraversalApplicationService : ITraversalApplicationService
{
    private readonly IGraphSearch _graphSearch;
    private readonly ILinkCalculatorFactory _linkCalculatorFactory;
    private readonly IPageRetrieverFactory _pageRetrieverFactory;
    private readonly IJobService _siteService;
    public TraversalApplicationService(
        IGraphSearch graphSearch,
        IPageRetrieverFactory pageRetrieverFactory,
        ILinkCalculatorFactory linkCalculatorFactory,
        IJobService siteService)
    {
        _graphSearch = graphSearch;
        _pageRetrieverFactory = pageRetrieverFactory;
        _linkCalculatorFactory = linkCalculatorFactory;
        _siteService = siteService;
    }

    public IAsyncEnumerable<string> TraverseAsync(ITraverseCommand command) =>
        _siteService.BuildJobAsync(command.NameOrRootUrl, command.FullScan, false, true, true).MapAsync(
                x => PagesAsync(x.job));

    private async IAsyncEnumerable<string> PagesAsync(Job job)
    {
        var rootUri = job.RootUrl;
        var adjacencyXPath = job.AdjacencyXPath;
        var pageRetriever = _pageRetrieverFactory.Build(job);
        var linkCalculator = _linkCalculatorFactory.Build(job);
        await foreach (var uri in Pages(rootUri, pageRetriever, adjacencyXPath, linkCalculator)
                           .Select(x => x.Uri.AbsoluteUri))
        {
            yield return uri;
        }
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
}
