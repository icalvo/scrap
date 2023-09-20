using System.Diagnostics.CodeAnalysis;
using Scrap.Common;
using Scrap.Domain.Resources;

namespace Scrap.Domain.Jobs;

public class Job
{
    public const int DefaultHttpRequestRetries = 5;
    public static readonly TimeSpan DefaultHttpRequestDelayBetweenRetries = TimeSpan.FromSeconds(1);

    internal Job(
        Uri rootUrl,
        ResourceType resourceType,
        AsyncLazy<IResourceRepositoryConfiguration> resourceRepository,
        XPath? adjacencyXPath = null,
        XPath? resourceXPath = null,
        int? httpRequestRetries = null,
        TimeSpan? httpRequestDelayBetweenRetries = null,
        bool? fullScan = null,
        bool? downloadAlways = null,
        bool? disableMarkingVisited = null,
        bool? disableResourceWrites = null)
    {
        AdjacencyXPath = adjacencyXPath;
        ResourceType = resourceType;
        DisableMarkingVisited = disableMarkingVisited ?? false;
        DisableResourceWrites = disableResourceWrites ?? false;
        ResourceXPath = resourceXPath;
        ResourceRepoArgs = resourceRepository;
        RootUrl = rootUrl;
        HttpRequestRetries = httpRequestRetries ?? DefaultHttpRequestRetries;
        HttpRequestDelayBetweenRetries = httpRequestDelayBetweenRetries ?? DefaultHttpRequestDelayBetweenRetries;
        FullScan = fullScan ?? false;
        DownloadAlways = downloadAlways ?? false;
    }

    public Uri RootUrl { get; }
    public XPath? AdjacencyXPath { get; }
    public XPath? ResourceXPath { get; }
    public AsyncLazy<IResourceRepositoryConfiguration> ResourceRepoArgs { get; }
    public int HttpRequestRetries { get; }
    public TimeSpan HttpRequestDelayBetweenRetries { get; }
    public bool DisableMarkingVisited { get; }
    public bool DisableResourceWrites { get; }
    public bool FullScan { get; }
    public bool DownloadAlways { get; }
    public ResourceType? ResourceType { get; }

    [MemberNotNull(nameof(ResourceXPath), nameof(ResourceRepoArgs))]
    public void ValidateResourceCapabilities()
    {
        if (ResourceXPath == null)
        {
            throw new InvalidOperationException("The job has no Resource XPath");
        }

        if (ResourceRepoArgs == null)
        {
            throw new InvalidOperationException("The job has no Resource Repository Configuration");
        }
    }
}
