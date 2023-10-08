using System.Diagnostics.CodeAnalysis;
using SharpX;

namespace Scrap.Domain.Jobs;

class TraverseJob : ITraverseJob
{
    public TraverseJob(
        Uri rootUrl,
        Maybe<XPath> adjacencyXPath,
        int httpRequestRetries,
        TimeSpan httpRequestDelayBetweenRetries,
        bool disableMarkingVisited,
        bool fullScan)
    {
        HttpRequestRetries = httpRequestRetries;
        HttpRequestDelayBetweenRetries = httpRequestDelayBetweenRetries;
        DisableMarkingVisited = disableMarkingVisited;
        FullScan = fullScan;
        RootUrl = rootUrl;
        AdjacencyXPath = adjacencyXPath;
    }

    public int HttpRequestRetries { get; }
    public TimeSpan HttpRequestDelayBetweenRetries { get; }
    public bool DisableMarkingVisited { get; }
    public bool FullScan { get; }
    public Uri RootUrl { get; }
    public Maybe<XPath> AdjacencyXPath { get; }
}
