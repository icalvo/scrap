using System;
using Microsoft.Extensions.Logging;
using Scrap.Resources;

namespace Scrap.JobDefinitions
{
    public record NewJobDefinitionDto(
        string Name,
        string AdjacencyXPath,
        string? AdjacencyAttribute,
        string ResourceXPath,
        string ResourceAttribute,
        IResourceRepositoryConfiguration ResourceRepoArgs,
        string? RootUrl,
        int? HttpRequestRetries,
        TimeSpan? HttpRequestDelayBetweenRetries,
        string? UrlPattern)
    {
        public void Log(ILogger logger)
        {
            logger.LogDebug("Name: {Name}", Name);
            logger.LogDebug("Root URL: {RootUrl}", RootUrl);
            logger.LogDebug("Adjacency X-Path: {AdjacencyXPath}", AdjacencyXPath);
            logger.LogDebug("Adjacency attribute: {AdjacencyAttribute}", AdjacencyAttribute);
            logger.LogDebug("Resource X-Path: {ResourceXPath}", ResourceXPath);
            logger.LogDebug("Resource attribute: {ResourceAttribute}", ResourceAttribute);
            logger.LogDebug("Resource repo args: {ResourceRepoArgs}", ResourceRepoArgs);
            logger.LogDebug("Url Pattern: {UrlPattern}", UrlPattern);
        }
    }
}
