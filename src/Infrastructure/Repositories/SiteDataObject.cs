using Scrap.Domain;
using Scrap.Domain.Resources;

namespace Scrap.Infrastructure.Repositories;

public record SiteDataObject(
    string Name,
    string? AdjacencyXPath = null,
    string? ResourceXPath = null,
    IResourceRepositoryConfiguration? ResourceRepository = null,
    string? RootUrl = null,
    int? HttpRequestRetries = null,
    TimeSpan? HttpRequestDelayBetweenRetries = null,
    string? UrlPattern = null,
    ResourceType? ResourceType = null)
{
    public bool HasResourceCapabilities() => ResourceXPath != null && ResourceRepository != null;
}
