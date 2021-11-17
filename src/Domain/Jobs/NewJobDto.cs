using System;
using Microsoft.Extensions.Logging;
using Scrap.JobDefinitions;
using Scrap.Resources;

namespace Scrap.Jobs
{
    public class NewJobDto
    {
        public NewJobDto(
            JobDefinitionDto jobDefinition,
            string? rootUrl,
            bool? whatIf,
            bool? fullScan,
            IResourceProcessorConfiguration? configuration)
            : this(
                jobDefinition.AdjacencyXPath,
                jobDefinition.AdjacencyAttribute,
                jobDefinition.ResourceXPath,
                jobDefinition.ResourceAttribute,
                configuration ?? jobDefinition.ResourceRepoArgs,
                rootUrl ?? jobDefinition.RootUrl ?? throw new ArgumentException("No root URL provided"),
                jobDefinition.HttpRequestRetries,
                jobDefinition.HttpRequestDelayBetweenRetries,
                whatIf,
                fullScan)
        {}

        public NewJobDto(string adjacencyXPath,
            string? adjacencyAttribute,
            string resourceXPath,
            string resourceAttribute,
            IResourceProcessorConfiguration resourceRepoArgs,
            string rootUrl,
            int? httpRequestRetries,
            TimeSpan? httpRequestDelayBetweenRetries,
            bool? whatIf,
            bool? fullScan)
        {
            AdjacencyXPath = adjacencyXPath;
            AdjacencyAttribute = adjacencyAttribute;
            ResourceXPath = resourceXPath;
            ResourceAttribute = resourceAttribute;
            ResourceRepoArgs = resourceRepoArgs;
            RootUrl = rootUrl;
            HttpRequestRetries = httpRequestRetries;
            HttpRequestDelayBetweenRetries = httpRequestDelayBetweenRetries;
            WhatIf = whatIf;
            FullScan = fullScan;
        }

        public string AdjacencyXPath { get; init; }
        public string? AdjacencyAttribute { get; init; }
        public string ResourceXPath { get; init; }
        public string ResourceAttribute { get; init; }
        public IResourceProcessorConfiguration ResourceRepoArgs { get; init; }
        public string RootUrl { get; init; }
        public int? HttpRequestRetries { get; init; }
        public TimeSpan? HttpRequestDelayBetweenRetries { get; init; }
        public bool? WhatIf { get; init; }
        public bool? FullScan { get; init; }

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
    }
}