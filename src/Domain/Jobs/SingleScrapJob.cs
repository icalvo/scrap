using Scrap.Common;
using Scrap.Domain.Resources;
using SharpX;

namespace Scrap.Domain.Jobs;

public class SingleScrapJob : ISingleScrapJob
{
    internal SingleScrapJob(
        Uri rootUrl,
        ResourceType resourceType,
        AsyncLazy<IResourceRepositoryConfiguration> resourceRepository,
        Maybe<XPath> adjacencyXPath,
        XPath resourceXPath,
        int httpRequestRetries,
        TimeSpan httpRequestDelayBetweenRetries,
        bool fullScan,
        bool downloadAlways,
        bool disableMarkingVisited,
        bool disableResourceWrites)
    {
        AdjacencyXPath = adjacencyXPath;
        ResourceType = resourceType;
        DisableMarkingVisited = disableMarkingVisited;
        DisableResourceWrites = disableResourceWrites;
        ResourceXPath = resourceXPath;
        ResourceRepoArgs = resourceRepository;
        RootUrl = rootUrl;
        HttpRequestRetries = httpRequestRetries;
        HttpRequestDelayBetweenRetries = httpRequestDelayBetweenRetries;
        FullScan = fullScan;
        DownloadAlways = downloadAlways;
    }

    public Uri RootUrl { get; }
    public Maybe<XPath> AdjacencyXPath { get; }

    public XPath ResourceXPath { get; }
    public AsyncLazy<IResourceRepositoryConfiguration> ResourceRepoArgs { get; }
    public int HttpRequestRetries { get; }
    public TimeSpan HttpRequestDelayBetweenRetries { get; }
    public bool DisableMarkingVisited { get; }
    public bool DisableResourceWrites { get; }
    public bool FullScan { get; }
    public bool DownloadAlways { get; }
    public ResourceType ResourceType { get; }
}
