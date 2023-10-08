using Scrap.Application.Download;
using Scrap.Common;
using Scrap.Common.Graphs;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using SharpX;

namespace Scrap.Application.Traversal;

public class TraversalApplicationService : ITraversalApplicationService
{
    private readonly IGraphSearch _graphSearch;
    private readonly ILinkCalculatorFactory _linkCalculatorFactory;
    private readonly IPageRetrieverFactory _pageRetrieverFactory;
    private readonly ICommandJobBuilder<ITraverseCommand, ITraverseJob> _siteFactory;
    
    public TraversalApplicationService(
        IGraphSearch graphSearch,
        IPageRetrieverFactory pageRetrieverFactory,
        ILinkCalculatorFactory linkCalculatorFactory,
        ICommandJobBuilder<ITraverseCommand, ITraverseJob> siteFactory)
    {
        _graphSearch = graphSearch;
        _pageRetrieverFactory = pageRetrieverFactory;
        _linkCalculatorFactory = linkCalculatorFactory;
        _siteFactory = siteFactory;
    }

    public IAsyncEnumerable<string> TraverseAsync(ITraverseCommand command) =>

        _siteFactory.Build(command)
            .MapAsync(x => PagesAsync(x.Item1));

        private async IAsyncEnumerable<string> PagesAsync(ITraverseJob job)
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
        Maybe<XPath> adjacencyXPath,
        ILinkCalculator linkCalculator) =>
        _graphSearch.SearchAsync(
            rootUri,
            pageRetriever.GetPageAsync,
            page => linkCalculator.CalculateLinks(page, adjacencyXPath));
}
