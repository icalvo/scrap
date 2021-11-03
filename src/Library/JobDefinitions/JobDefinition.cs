using System;
using Microsoft.Extensions.Logging;

namespace Scrap.JobDefinitions
{
    public class JobDefinition
    {
        public JobDefinition(string adjacencyXPath, string adjacencyAttribute, string resourceXPath, string resourceAttribute, string resourceRepoType, string[] resourceRepoArgs, string? rootUrl, int httpRequestRetries, TimeSpan httpRequestDelayBetweenRetries)
        {
            AdjacencyXPath = adjacencyXPath;
            AdjacencyAttribute = adjacencyAttribute;
            ResourceXPath = resourceXPath;
            ResourceAttribute = resourceAttribute;
            ResourceRepoType = resourceRepoType;
            ResourceRepoArgs = resourceRepoArgs;
            RootUrl = rootUrl;
            HttpRequestRetries = httpRequestRetries;
            HttpRequestDelayBetweenRetries = httpRequestDelayBetweenRetries;
        }

        public JobDefinition(JobDefinition jobDefinition)
            : this(jobDefinition.AdjacencyXPath, jobDefinition.AdjacencyAttribute, jobDefinition.ResourceXPath, jobDefinition.ResourceAttribute, jobDefinition.ResourceRepoType, jobDefinition.ResourceRepoArgs, jobDefinition.RootUrl, jobDefinition.HttpRequestRetries, jobDefinition.HttpRequestDelayBetweenRetries)
        {
        }

        public JobDefinition(JobDefinition jobDefinition, string? rootUrl)
            : this(jobDefinition.AdjacencyXPath, jobDefinition.AdjacencyAttribute, jobDefinition.ResourceXPath, jobDefinition.ResourceAttribute, jobDefinition.ResourceRepoType, jobDefinition.ResourceRepoArgs, jobDefinition.RootUrl, jobDefinition.HttpRequestRetries, jobDefinition.HttpRequestDelayBetweenRetries)
        {
            if (rootUrl != null)
            {
                RootUrl = rootUrl;
            }
        }

        public void Log(ILogger logger)
        {
            logger.LogDebug("Root URL: {RootUrl}", RootUrl);
            logger.LogDebug("Adjacency X-Path: {AdjacencyXPath}", AdjacencyXPath);
            logger.LogDebug("Adjacency attribute: {AdjacencyAttribute}", AdjacencyAttribute);
            logger.LogDebug("Resource X-Path: {ResourceXPath}", ResourceXPath);
            logger.LogDebug("Resource attribute: {ResourceAttribute}", ResourceAttribute);
            logger.LogDebug("Resource repo type: {ResourceRepoType}", ResourceRepoType);
            logger.LogDebug("Resource repo args: {ResourceRepoArgs}", string.Join(" , ", ResourceRepoArgs));
        }

        public string? RootUrl { get; }
        public string AdjacencyXPath { get; }
        public string AdjacencyAttribute { get; }
        public string ResourceXPath { get; }
        public string ResourceAttribute { get; }
        public string ResourceRepoType { get; }
        public string[] ResourceRepoArgs { get; }
        public int HttpRequestRetries { get; }
        public TimeSpan HttpRequestDelayBetweenRetries { get; }
    }
}