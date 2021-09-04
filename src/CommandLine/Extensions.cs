using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrap.CommandLine
{
    public static class Extensions
    {
        public static string[] CleanSegments(this Uri uri)
        {
            return uri.Segments.Select(segment => segment.Replace("/", "")).ToArray();
        }

        public static string Extension(this Uri uri)
        {
            return Path.GetExtension(uri.Segments.Last());
        }

        public static IEnumerable<string> C(this string first, IEnumerable<string> second)
        {
            return new[] { first }.Concat(second);
        }

        public static IEnumerable<string> C(this IEnumerable<string> first, IEnumerable<string> second)
        {
            return first.Concat(second);
        }

        public static IEnumerable<string> C(this string first, string second)
        {
            return new[] { first }.Concat(new[] { second });
        }

        public static IEnumerable<string> C(this IEnumerable<string> first, string second)
        {
            return first.Concat(new [] { second });
        }
    }
}