using Scrap.Domain.Resources;

namespace Scrap.Domain.JobDefinitions;

public record JobDefinitionDto(
    Guid Id,
    string Name,
    string? AdjacencyXPath,
    string? ResourceXPath,
    IResourceRepositoryConfiguration? ResourceRepository,
    string? RootUrl,
    int? HttpRequestRetries,
    TimeSpan? HttpRequestDelayBetweenRetries,
    string? UrlPattern,
    ResourceType? ResourceType)
{
    public bool HasResourceCapabilities()
    {
        return ResourceXPath != null && ResourceRepository != null;
    }
}
