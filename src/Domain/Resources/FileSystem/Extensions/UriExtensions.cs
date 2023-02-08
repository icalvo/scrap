namespace Scrap.Domain.Resources.FileSystem.Extensions;

public static class UriExtensions
{
    public static string[] CleanSegments(this Uri uri) =>
        uri.Segments.Select(segment => segment.Replace("/", "")).ToArray();

    public static string Extension(this Uri uri) => Path.GetExtension(uri.Segments.Last());
}
