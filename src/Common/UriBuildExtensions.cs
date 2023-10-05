namespace Scrap.Common;

public static class UriBuildExtensions
{
    public static Uri? TryBuildUri(this string? url) => url == null ? null : new Uri(url);
}
