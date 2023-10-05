using System.Diagnostics.CodeAnalysis;
using Scrap.Common;
using Scrap.Domain.Resources;

namespace Scrap.Domain.Jobs;

public interface IResourceRepositoryOptions
{
    AsyncLazy<IResourceRepositoryConfiguration> ResourceRepoArgs { get; }
    bool DisableResourceWrites { get; }
}

public interface IDownloadStreamProviderOptions : IAsyncPolicyOptions
{
}

public interface IAsyncPolicyOptions
{
    public int HttpRequestRetries { get; }
    public TimeSpan HttpRequestDelayBetweenRetries { get; }
}

public interface IPageRetrieverOptions : IDownloadStreamProviderOptions
{
}

public interface IVisitedPageRepositoryOptions
{
    public bool DisableMarkingVisited { get; }
}

public interface ILinkCalculatorOptions : IVisitedPageRepositoryOptions
{
    public bool FullScan { get; }
}

public interface IDownloadJob : IResourceRepositoryOptions, IPageRetrieverOptions
{
    public bool DownloadAlways { get; }
}

public class DownloadJob : IDownloadJob
{
    public DownloadJob(
        AsyncLazy<IResourceRepositoryConfiguration> resourceRepoArgs,
        bool disableResourceWrites,
        int httpRequestRetries,
        TimeSpan httpRequestDelayBetweenRetries,
        bool downloadAlways)
    {
        ResourceRepoArgs = resourceRepoArgs;
        DisableResourceWrites = disableResourceWrites;
        HttpRequestRetries = httpRequestRetries;
        HttpRequestDelayBetweenRetries = httpRequestDelayBetweenRetries;
        DownloadAlways = downloadAlways;
    }

    public AsyncLazy<IResourceRepositoryConfiguration> ResourceRepoArgs { get; }
    public bool DisableResourceWrites { get; }
    public int HttpRequestRetries { get; }
    public TimeSpan HttpRequestDelayBetweenRetries { get; }
    public bool DownloadAlways { get; }
}

public interface IResourcesJob : IPageRetrieverOptions
{
    public XPath ResourceXPath { get; }
    void ValidateResourceCapabilities();
}

public interface ISingleScrapJob : IResourceRepositoryOptions, ILinkCalculatorOptions, IResourcesJob
{
    public ResourceType? ResourceType { get; }
    public Uri RootUrl { get; }
    public XPath? AdjacencyXPath { get; }
    public bool DownloadAlways { get; }
}

public class Job : IDownloadJob, ISingleScrapJob
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
        _resourceXPath = resourceXPath;
        ResourceRepoArgs = resourceRepository;
        RootUrl = rootUrl;
        HttpRequestRetries = httpRequestRetries ?? DefaultHttpRequestRetries;
        HttpRequestDelayBetweenRetries = httpRequestDelayBetweenRetries ?? DefaultHttpRequestDelayBetweenRetries;
        FullScan = fullScan ?? false;
        DownloadAlways = downloadAlways ?? false;
    }

    public Uri RootUrl { get; }
    public XPath? AdjacencyXPath { get; }
    private XPath? _resourceXPath;

    public XPath ResourceXPath => _resourceXPath ?? throw new InvalidOperationException("The job has no Resource XPath");
    public AsyncLazy<IResourceRepositoryConfiguration> ResourceRepoArgs { get; }
    public int HttpRequestRetries { get; }
    public TimeSpan HttpRequestDelayBetweenRetries { get; }
    public bool DisableMarkingVisited { get; }
    public bool DisableResourceWrites { get; }
    public bool FullScan { get; }
    public bool DownloadAlways { get; }
    public ResourceType? ResourceType { get; }

    [MemberNotNull(nameof(_resourceXPath), nameof(ResourceRepoArgs))]
    public void ValidateResourceCapabilities()
    {
        if (_resourceXPath == null)
        {
            throw new InvalidOperationException("The job has no Resource XPath");
        }

        if (ResourceRepoArgs == null)
        {
            throw new InvalidOperationException("The job has no Resource Repository Configuration");
        }
    }
}
