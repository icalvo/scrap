﻿using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Jobs.Graphs;
using Scrap.Domain.Pages;

namespace Scrap.Application;

public class TraversalApplicationService : ITraversalApplicationService
{
    private readonly IGraphSearch _graphSearch;
    private readonly IAsyncFactory<JobDto, Job> _jobFactory;
    private readonly IFactory<Job, ILinkCalculator> _linkCalculatorFactory;
    private readonly IFactory<Job, IPageRetriever> _pageRetrieverFactory;

    public TraversalApplicationService(
        IGraphSearch graphSearch,
        IFactory<Job, IPageRetriever> pageRetrieverFactory,
        IFactory<Job, ILinkCalculator> linkCalculatorFactory,
        IAsyncFactory<JobDto, Job> jobFactory)
    {
        _graphSearch = graphSearch;
        _pageRetrieverFactory = pageRetrieverFactory;
        _linkCalculatorFactory = linkCalculatorFactory;
        _jobFactory = jobFactory;
    }

    public async IAsyncEnumerable<string> TraverseAsync(JobDto jobDto)
    {
        var job = await _jobFactory.Build(jobDto);
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
