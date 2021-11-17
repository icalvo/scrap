using System;
using Microsoft.Extensions.Logging;
using Scrap.Resources;

namespace Scrap.JobDefinitions
{
    public class JobDefinition
    {
        public JobDefinition(NewJobDefinitionDto dto)
        {
            Id = new JobDefinitionId();
            Name = dto.Name;
            AdjacencyXPath = dto.AdjacencyXPath;
            AdjacencyAttribute = dto.AdjacencyAttribute ?? "href";
            ResourceXPath = dto.ResourceXPath;
            ResourceAttribute = dto.ResourceAttribute;
            ResourceRepoArgs = dto.ResourceRepoArgs;
            UrlPattern = dto.UrlPattern;
            RootUrl = dto.RootUrl;
            HttpRequestRetries = dto.HttpRequestRetries;
            HttpRequestDelayBetweenRetries = dto.HttpRequestDelayBetweenRetries;
        }

        public JobDefinition(JobDefinitionDto dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            AdjacencyXPath = dto.AdjacencyXPath;
            AdjacencyAttribute = dto.AdjacencyAttribute;
            ResourceXPath = dto.ResourceXPath;
            ResourceAttribute = dto.ResourceAttribute;
            ResourceRepoArgs = dto.ResourceRepoArgs;
            UrlPattern = dto.UrlPattern;
            RootUrl = dto.RootUrl;
            HttpRequestRetries = dto.HttpRequestRetries;
            HttpRequestDelayBetweenRetries = dto.HttpRequestDelayBetweenRetries;
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
                AdjacencyXPath,
                AdjacencyAttribute,
                ResourceXPath,
                ResourceAttribute,
                ResourceRepoArgs,
                RootUrl,
                HttpRequestRetries,
                HttpRequestDelayBetweenRetries,
                UrlPattern);            
        }

        public void Log(ILogger logger)
        {
            logger.LogDebug("Root URL: {RootUrl}", RootUrl);
            logger.LogDebug("Adjacency X-Path: {AdjacencyXPath}", AdjacencyXPath);
            logger.LogDebug("Adjacency attribute: {AdjacencyAttribute}", AdjacencyAttribute);
            logger.LogDebug("Resource X-Path: {ResourceXPath}", ResourceXPath);
            logger.LogDebug("Resource attribute: {ResourceAttribute}", ResourceAttribute);
            logger.LogDebug("Resource repo args: {ResourceRepoArgs}", ResourceRepoArgs);
            logger.LogDebug("Url Pattern: {UrlPattern}", UrlPattern);
        }

        public JobDefinitionId Id { get; }
        public string? Name { get; private set; }
        public string? RootUrl { get; private set; }
        public string AdjacencyXPath { get; private set; }
        public string? AdjacencyAttribute { get; private set; }
        public string ResourceXPath { get; private set; }
        public string ResourceAttribute { get; private set; }
        public IResourceProcessorConfiguration ResourceRepoArgs { get; private set; }
        public int? HttpRequestRetries { get; private set; }
        public TimeSpan? HttpRequestDelayBetweenRetries { get; private set; }
        public string? UrlPattern { get; private set; }

        public void SetValues(NewJobDefinitionDto dto)
        {
            Name = dto.Name;
            AdjacencyXPath = dto.AdjacencyXPath;
            AdjacencyAttribute = dto.AdjacencyAttribute;
            ResourceXPath = dto.ResourceXPath;
            ResourceAttribute = dto.ResourceAttribute;
            ResourceRepoArgs = dto.ResourceRepoArgs;
            UrlPattern = dto.UrlPattern;
            RootUrl = dto.RootUrl;
            HttpRequestRetries = dto.HttpRequestRetries;
            HttpRequestDelayBetweenRetries = dto.HttpRequestDelayBetweenRetries;
        }
    }
}