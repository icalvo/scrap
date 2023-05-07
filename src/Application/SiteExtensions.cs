using Scrap.Domain;
using Scrap.Domain.Sites;

namespace Scrap.Application;

internal static class SiteExtensions
{
    public static Site ToSite(this SiteDto dto) =>
        new(
            dto.Name,
            dto.ResourceType,
            dto.RootUrl == null ? null : new Uri(dto.RootUrl),
            dto.AdjacencyXPath == null ? null : new XPath(dto.AdjacencyXPath),
            dto.ResourceXPath == null ? null : new XPath(dto.ResourceXPath),
            dto.ResourceRepository,
            dto.HttpRequestRetries,
            dto.HttpRequestDelayBetweenRetries,
            dto.UrlPattern);

    public static SiteDto ToDto(this Site site) =>
        new(
            site.Name,
            site.AdjacencyXPath?.ToString(),
            site.ResourceXPath?.ToString(),
            site.ResourceRepoArgs,
            site.RootUrl?.AbsoluteUri,
            site.HttpRequestRetries,
            site.HttpRequestDelayBetweenRetries,
            site.UrlPattern,
            site.ResourceType);
}
