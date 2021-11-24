using System;
using Scrap.Resources;

namespace Scrap.JobDefinitions
{
    public record JobDefinitionDto(
        Guid Id,
        string Name,
        string? AdjacencyXPath,
        string ResourceXPath,
        IResourceRepositoryConfiguration ResourceRepository,
        string? RootUrl,
        int? HttpRequestRetries,
        TimeSpan? HttpRequestDelayBetweenRetries,
        string? UrlPattern,
        ResourceType? ResourceType)
    {
    }
}
