using Microsoft.Extensions.Logging;
using Scrap.Domain.Resources;

namespace Scrap.Domain.JobDefinitions;

public class JobDefinition
{
    public JobDefinition(NewJobDefinitionDto dto)
    {
        Id = new JobDefinitionId();
        Name = dto.Name;
        AdjacencyXPath = dto.AdjacencyXPath == null ? null : new XPath(dto.AdjacencyXPath);
        ResourceXPath = dto.ResourceXPath;
        ResourceRepoArgs = dto.ResourceRepository;
        UrlPattern = dto.UrlPattern;
        RootUrl = dto.RootUrl;
        HttpRequestRetries = dto.HttpRequestRetries;
        HttpRequestDelayBetweenRetries = dto.HttpRequestDelayBetweenRetries;
        ResourceType = dto.ResourceType;
    }

    public JobDefinition(JobDefinitionDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        AdjacencyXPath = dto.AdjacencyXPath == null ? null : new XPath(dto.AdjacencyXPath);
        ResourceXPath = dto.ResourceXPath == null ? null : new XPath(dto.ResourceXPath);
        ResourceRepoArgs = dto.ResourceRepository;
        UrlPattern = dto.UrlPattern;
        RootUrl = dto.RootUrl;
        HttpRequestRetries = dto.HttpRequestRetries;
        HttpRequestDelayBetweenRetries = dto.HttpRequestDelayBetweenRetries;
        ResourceType = dto.ResourceType ?? default(ResourceType);
    }

    public JobDefinition(JobDefinitionDto dto, string? rootUrl)
        : this(dto with { RootUrl = rootUrl ?? dto.RootUrl })
    {
    }

    public JobDefinitionId Id { get; }
    public string Name { get; private set; }
    public string? RootUrl { get; private set; }
    public XPath? AdjacencyXPath { get; private set; }
    public XPath? ResourceXPath { get; private set; }
    public IResourceRepositoryConfiguration? ResourceRepoArgs { get; private set; }
    public int? HttpRequestRetries { get; private set; }
    public TimeSpan? HttpRequestDelayBetweenRetries { get; private set; }
    public string? UrlPattern { get; private set; }
    public ResourceType ResourceType { get; }

    public JobDefinitionDto ToDto() =>
        new JobDefinitionDto(
            Id,
            Name,
            AdjacencyXPath?.ToString(),
            ResourceXPath?.ToString(),
            ResourceRepoArgs,
            RootUrl,
            HttpRequestRetries,
            HttpRequestDelayBetweenRetries,
            UrlPattern,
            ResourceType);

    public void Log(ILogger logger, LogLevel logLevel)
    {
        logger.Log(logLevel, "Name: {Name}", Name);
        logger.Log(logLevel, "Root URL: {RootUrl}", RootUrl);
        logger.Log(logLevel, "Adjacency XPath: {AdjacencyXPath}", AdjacencyXPath);
        logger.Log(logLevel, "Resource XPath: {ResourceXPath}", ResourceXPath);
        logger.Log(logLevel, "Resource repo args:\n{ResourceRepoArgs}", ResourceRepoArgs);
        logger.Log(logLevel, "Url Pattern: {UrlPattern}", UrlPattern);
        logger.Log(logLevel, "Resource Type: {ResourceType}", ResourceType);
    }

    public void SetValues(NewJobDefinitionDto dto)
    {
        Name = dto.Name;
        AdjacencyXPath = dto.AdjacencyXPath == null ? null : new XPath(dto.AdjacencyXPath);
        ResourceXPath = dto.ResourceXPath;
        ResourceRepoArgs = dto.ResourceRepository;
        UrlPattern = dto.UrlPattern;
        RootUrl = dto.RootUrl;
        HttpRequestRetries = dto.HttpRequestRetries;
        HttpRequestDelayBetweenRetries = dto.HttpRequestDelayBetweenRetries;
    }
}
