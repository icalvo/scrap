using System.Diagnostics.CodeAnalysis;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Resources;

namespace Scrap.Domain.Jobs;

public class NewJobDto
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private NewJobDto()
    {
    }

    public NewJobDto(
        JobDefinitionDto jobDefinition,
        string? rootUrl,
        bool? fullScan,
        IResourceRepositoryConfiguration? configuration,
        bool? downloadAlways, 
        bool? disableMarkingVisited,
        bool? disableResourceWrites)
        : this(
            jobDefinition.AdjacencyXPath,
            jobDefinition.ResourceXPath,
            configuration ?? jobDefinition.ResourceRepository,
            rootUrl ?? jobDefinition.RootUrl ?? throw new ArgumentException("No root URL provided"),
            jobDefinition.HttpRequestRetries,
            jobDefinition.HttpRequestDelayBetweenRetries,
            fullScan,
            downloadAlways,
            jobDefinition.ResourceType ?? default(ResourceType), disableMarkingVisited, disableResourceWrites)
    {}

    public NewJobDto(
        string? adjacencyXPath,
        string resourceXPath,
        IResourceRepositoryConfiguration resourceRepository,
        string rootUrl,
        int? httpRequestRetries,
        TimeSpan? httpRequestDelayBetweenRetries,
        bool? fullScan,
        bool? downloadAlways,
        ResourceType resourceType, bool? disableMarkingVisited, bool? disableResourceWrites)
    {
        AdjacencyXPath = adjacencyXPath;
        ResourceType = resourceType;
        DisableMarkingVisited = disableMarkingVisited;
        DisableResourceWrites = disableResourceWrites;
        ResourceXPath = resourceXPath;
        ResourceRepository = resourceRepository;
        RootUrl = rootUrl;
        HttpRequestRetries = httpRequestRetries;
        HttpRequestDelayBetweenRetries = httpRequestDelayBetweenRetries;
        FullScan = fullScan;
        DownloadAlways = downloadAlways;
    }

    public string? AdjacencyXPath { get; }
    public string ResourceXPath { get; } = null!;
    public IResourceRepositoryConfiguration ResourceRepository { get; } = null!;
    public string RootUrl { get; } = null!;
    public int? HttpRequestRetries { get; }
    public TimeSpan? HttpRequestDelayBetweenRetries { get; }
    public bool? FullScan { get; }
    public bool? DownloadAlways { get; }
    public ResourceType ResourceType { get; }
    public bool? DisableMarkingVisited { get; }
    public bool? DisableResourceWrites { get; }
}
