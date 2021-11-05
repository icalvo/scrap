using System;
using Microsoft.Extensions.Logging;
using Scrap.Resources;

namespace Scrap.JobDefinitions
{
    public class JobDefinition
    {
        public JobDefinition(JobDefinitionDto jobDefinition)
        {
            AdjacencyXPath = jobDefinition.AdjacencyXPath;
            AdjacencyAttribute = jobDefinition.AdjacencyAttribute ?? "href";
            ResourceXPath = jobDefinition.ResourceXPath;
            ResourceAttribute = jobDefinition.ResourceAttribute;
            ResourceRepoArgs = jobDefinition.ResourceRepoArgs;
            RootUrl = jobDefinition.RootUrl;
            HttpRequestRetries = jobDefinition.HttpRequestRetries ?? 5;
            HttpRequestDelayBetweenRetries = jobDefinition.HttpRequestDelayBetweenRetries ?? TimeSpan.FromSeconds(1);
            WhatIf = jobDefinition.WhatIf ?? false;
        }

        public JobDefinition(JobDefinitionDto jobDefinition, string? rootUrl)
            : this(jobDefinition with { RootUrl = rootUrl ?? jobDefinition.RootUrl })
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
                WhatIf);            
        }

        public void Log(ILogger logger)
        {
            logger.LogDebug("Root URL: {RootUrl}", RootUrl);
            logger.LogDebug("Adjacency X-Path: {AdjacencyXPath}", AdjacencyXPath);
            logger.LogDebug("Adjacency attribute: {AdjacencyAttribute}", AdjacencyAttribute);
            logger.LogDebug("Resource X-Path: {ResourceXPath}", ResourceXPath);
            logger.LogDebug("Resource attribute: {ResourceAttribute}", ResourceAttribute);
            logger.LogDebug("Resource repo args: {ResourceRepoArgs}", ResourceRepoArgs);
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
    }
}