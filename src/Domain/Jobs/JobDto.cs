using System;
using Microsoft.Extensions.Logging;
using Scrap.Resources;

namespace Scrap.Jobs
{
    public record JobDto(
        Guid Id,
        string AdjacencyXPath,
        string AdjacencyAttribute,
        string ResourceXPath,
        string ResourceAttribute,
        IResourceRepositoryConfiguration ResourceRepoArgs,
        string RootUrl,
        int HttpRequestRetries,
        TimeSpan HttpRequestDelayBetweenRetries,
        bool WhatIf,
        bool FullScan)
    {
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