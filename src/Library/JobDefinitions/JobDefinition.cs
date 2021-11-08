using System;
using Microsoft.Extensions.Logging;
using Scrap.Resources;

namespace Scrap.JobDefinitions
{
    public class JobDefinition
    {
        public JobDefinition(JobDefinitionDto dto)
        {
            AdjacencyXPath = dto.AdjacencyXPath;
            AdjacencyAttribute = dto.AdjacencyAttribute ?? "href";
            ResourceXPath = dto.ResourceXPath;
            ResourceAttribute = dto.ResourceAttribute;
            ResourceRepoArgs = dto.ResourceRepoArgs;
            UrlPattern = dto.UrlPattern;
            RootUrl = dto.RootUrl;
            HttpRequestRetries = dto.HttpRequestRetries ?? 5;
            HttpRequestDelayBetweenRetries = dto.HttpRequestDelayBetweenRetries ?? TimeSpan.FromSeconds(1);
            WhatIf = dto.WhatIf ?? false;
            FullScan = dto.FullScan ?? false;
        }

        public JobDefinition(JobDefinitionDto dto, string? rootUrl)
            : this(dto with { RootUrl = rootUrl ?? dto.RootUrl })
        {
        }

        public JobDefinitionDto ToDto()
        {
            return new JobDefinitionDto(
                AdjacencyXPath,
                AdjacencyAttribute,
                ResourceXPath,
                ResourceAttribute,
                ResourceRepoArgs,
                RootUrl,
                HttpRequestRetries,
                HttpRequestDelayBetweenRetries,
                WhatIf,
                FullScan,
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
            logger.LogDebug("What if flag: {WhatIf}", WhatIf);
            logger.LogDebug("Full scan flag: {FullScan}", FullScan);
        }

        public string? RootUrl { get; }
        public string AdjacencyXPath { get; }
        public string AdjacencyAttribute { get; }
        public string ResourceXPath { get; }
        public string ResourceAttribute { get; }
        public IResourceRepositoryConfiguration ResourceRepoArgs { get; }
        public int HttpRequestRetries { get; }
        public TimeSpan HttpRequestDelayBetweenRetries { get; }
        public bool WhatIf { get; }
        public bool FullScan { get; }
        public string? UrlPattern { get; }
    }
}