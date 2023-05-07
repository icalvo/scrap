using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Scrap.Domain.Resources;
using Scrap.Domain.Sites;

namespace Scrap.Domain.Jobs;

public class Job
{
    public const int DefaultHttpRequestRetries = 5;
    public static readonly TimeSpan DefaultHttpRequestDelayBetweenRetries = TimeSpan.FromSeconds(1);

    internal Job(
        Site site,
        Uri? rootUrl = null,
        bool? fullScan = null,
        IResourceRepositoryConfiguration? configuration = null,
        bool? downloadAlways = null,
        bool? disableMarkingVisited = null,
        bool? disableResourceWrites = null) : this(
        rootUrl ?? site.RootUrl ?? throw new ArgumentException("No root URL provided", nameof(site)),
        site.ResourceType,
        site.AdjacencyXPath,
        site.ResourceXPath,
        configuration ?? site.ResourceRepoArgs,
        site.HttpRequestRetries,
        site.HttpRequestDelayBetweenRetries,
        fullScan,
        downloadAlways,
        disableMarkingVisited,
        disableResourceWrites)
    {
    }

    internal Job(
        Uri rootUrl,
        ResourceType resourceType,
        XPath? adjacencyXPath = null,
        XPath? resourceXPath = null,
        IResourceRepositoryConfiguration? resourceRepository = null,
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
    public IResourceRepositoryConfiguration? ResourceRepoArgs { get; }
    public int HttpRequestRetries { get; }
    public TimeSpan HttpRequestDelayBetweenRetries { get; }
    public bool DisableMarkingVisited { get; }
    public bool DisableResourceWrites { get; }
    public bool FullScan { get; }
    public bool DownloadAlways { get; }
    public ResourceType? ResourceType { get; }

    public void Log(ILogger logger, LogLevel logLevel)
    {
        logger.Log(logLevel, "Root URL: {RootUrl}", RootUrl);
        logger.Log(logLevel, "Adjacency XPath: {AdjacencyXPath}", AdjacencyXPath);
        logger.Log(logLevel, "Resource XPath: {ResourceXPath}", ResourceXPath);
        logger.Log(logLevel, "Resource repo args:\n{ResourceRepoArgs}", ResourceRepoArgs);
        logger.Log(logLevel, "Disable marking visited pages: {DisableMarkingVisited}", DisableMarkingVisited);
        logger.Log(logLevel, "Disable writing resources: {DisableResourceWrites}", DisableResourceWrites);
        logger.Log(logLevel, "Full scan flag: {FullScan}", FullScan);
        logger.Log(logLevel, "Download always flag: {DownloadAlways}", DownloadAlways);
        logger.Log(logLevel, "HTTP request retries: {HttpRequestRetries}", HttpRequestRetries);
        logger.Log(
            logLevel,
            "HTTP request delay between retries: {HttpRequestDelayBetweenRetries}",
            HttpRequestDelayBetweenRetries);
    }

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
