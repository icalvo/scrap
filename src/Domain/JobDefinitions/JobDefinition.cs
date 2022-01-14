using System;
using Microsoft.Extensions.Logging;
using Scrap.Resources;

namespace Scrap.JobDefinitions;

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
        ResourceXPath = dto.ResourceXPath;
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

    public JobDefinitionDto ToDto()
    {
        return new JobDefinitionDto(
            Id,
            Name,
            AdjacencyXPath?.ToString(),
            ResourceXPath.ToString(),
            ResourceRepoArgs,
            RootUrl,
            HttpRequestRetries,
            HttpRequestDelayBetweenRetries,
            UrlPattern,
            ResourceType);            
    }

    public void Log(ILogger logger)
    {
        logger.LogDebug("Name: {Name}", Name);
        logger.LogDebug("Root URL: {RootUrl}", RootUrl);
        logger.LogDebug("Adjacency XPath: {AdjacencyXPath}", AdjacencyXPath);
        logger.LogDebug("Resource XPath: {ResourceXPath}", ResourceXPath);
        logger.LogDebug("Resource repo args: {ResourceRepoArgs}", ResourceRepoArgs);
        logger.LogDebug("Url Pattern: {UrlPattern}", UrlPattern);
        logger.LogDebug("Resource Type: {ResourceType}", ResourceType);
    }

    public JobDefinitionId Id { get; }
    public string Name { get; private set; }
    public string? RootUrl { get; private set; }
    public XPath? AdjacencyXPath { get; private set; }
    public XPath ResourceXPath { get; private set; }
    public IResourceRepositoryConfiguration ResourceRepoArgs { get; private set; }
    public int? HttpRequestRetries { get; private set; }
    public TimeSpan? HttpRequestDelayBetweenRetries { get; private set; }
    public string? UrlPattern { get; private set; }
    public ResourceType ResourceType { get; private set; }
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

public enum ResourceType
{
    DownloadLink = 0,
    Text = 1
}
