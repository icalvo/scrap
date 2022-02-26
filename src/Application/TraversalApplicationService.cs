using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Jobs.Graphs;
using Scrap.Domain.Pages;

namespace Scrap.Application;

public class TraversalApplicationService : ITraversalApplicationService
{
    private readonly IGraphSearch _graphSearch;
    private readonly IPageRetriever _pageRetriever;
    private readonly ILinkCalculator _linkCalculator;
    private readonly IJobFactory _jobFactory;

    public TraversalApplicationService(
        IGraphSearch graphSearch,
        IPageRetriever pageRetriever,
        ILinkCalculator linkCalculator, IJobFactory jobFactory)
    {
        _graphSearch = graphSearch;
        _pageRetriever = pageRetriever;
        _linkCalculator = linkCalculator;
        _jobFactory = jobFactory;
    }

    public async IAsyncEnumerable<string> TraverseAsync(NewJobDto jobDto)
    {
        var job = await _jobFactory.CreateAsync(jobDto);
        var rootUri = job.RootUrl;
        var adjacencyXPath = job.AdjacencyXPath;
        await foreach (var uri in Pages(rootUri, _pageRetriever, adjacencyXPath, _linkCalculator)
                           .Select(x => x.Uri.AbsoluteUri))
        {
            yield return uri;
        }
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
}