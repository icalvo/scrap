using Scrap.Common;
using Scrap.Domain;
using Scrap.Domain.Sites;
using SharpX;

namespace Scrap.Application;

internal static class SiteExtensions
{
    public static Site ToSite(this SiteDto dto) =>
        new(
            dto.Name,
            dto.ResourceType,
            dto.RootUrl.TryBuildUri(),
            dto.AdjacencyXPath.TryBuildXPath(),
            dto.ResourceXPath.TryBuildXPath(),
            dto.ResourceRepository,
            dto.HttpRequestRetries,
            dto.HttpRequestDelayBetweenRetries,
            dto.UrlPattern);

    private static XPath? TryBuildXPath(this string? xpath) => xpath == null ? null : new XPath(xpath);
    
    public static SiteDto ToDto(this Site site) =>
        new(
            site.Name,
            site.AdjacencyXPath.FromJust()?.ToString(),
            site.ResourceXPath.FromJust()?.ToString(),
            site.ResourceRepoArgs.FromJust(),
            site.RootUrl.FromJust()?.AbsoluteUri,
            site.HttpRequestRetries.FromJust(),
            site.HttpRequestDelayBetweenRetries.FromJust(),
            site.UrlPattern.FromJust(),
            site.ResourceType);
}
