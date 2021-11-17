using System;
using System.IO;
using System.Linq;

namespace Scrap.Resources.FileSystem.Extensions
{
    public static class UriExtensions
    {
        public static string[] CleanSegments(this Uri uri)
        {
            return uri.Segments.Select(segment => segment.Replace("/", "")).ToArray();
        }

        public static string Extension(this Uri uri)
        {
            return Path.GetExtension(uri.Segments.Last());
        }
    }
}