using Microsoft.Extensions.Logging;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Resources;

namespace Scrap.Domain.Jobs;

public class Job
{
    public const int DefaultHttpRequestRetries = 5;
    public static readonly TimeSpan DefaultHttpRequestDelayBetweenRetries = TimeSpan.FromSeconds(1);

    public Job(JobDto dto)
    {
        Id = new JobId();
        AdjacencyXPath = dto.AdjacencyXPath == null ? null : new XPath(dto.AdjacencyXPath);
        ResourceXPath = dto.ResourceXPath == null ? null : new XPath(dto.ResourceXPath);
        ResourceRepoArgs = dto.ResourceRepository;
        RootUrl = new Uri(dto.RootUrl ?? throw new ArgumentException("Root URL must not be null", nameof(dto)));
        HttpRequestRetries = dto.HttpRequestRetries ?? DefaultHttpRequestRetries;
        HttpRequestDelayBetweenRetries = dto.HttpRequestDelayBetweenRetries ?? DefaultHttpRequestDelayBetweenRetries;
        DisableMarkingVisited = dto.DisableMarkingVisited ?? false;
        DisableResourceWrites = dto.DisableResourceWrites ?? false;
        FullScan = dto.FullScan ?? false;
        DownloadAlways = dto.DownloadAlways ?? false;
        ResourceType = dto.ResourceType;
    }

    public JobId Id { get; }
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
    public ResourceType ResourceType { get; }

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
        logger.Log(logLevel, "HTTP request delay between retries: {HttpRequestDelayBetweenRetries}",
            HttpRequestDelayBetweenRetries);
    }

    public (XPath ResourceXPath, IResourceRepositoryConfiguration ResourceRepoArgs) GetResourceCapabilitiesOrThrow()
    {
        if (ResourceXPath == null)
        {
            throw new InvalidOperationException("The job has no Resource XPath");
        }

        if (ResourceRepoArgs == null)
        {
            throw new InvalidOperationException("The job has no Resource Repository Configuration");
        }

        return (ResourceXPath, ResourceRepoArgs);
    }
}
