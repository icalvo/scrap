using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrap
{
    public static class StringExtensions
    {
        public static IEnumerable<string> C(this string first, IEnumerable<string> second)
        {
            return new[] { first }.Concat(second);
        }

        public static IEnumerable<string> C(this IEnumerable<string> first, IEnumerable<string> second)
        {
            return first.Concat(second);
        }

        public static IEnumerable<string> C(this string first, string? second)
        {
            return second == null ? new[] { first } : new[] { first }.Concat(new[] { second });
        }

        public static IEnumerable<string> C(this IEnumerable<string> first, string? second)
        {
            return second == null ? first : first.Concat(new [] { second });
        }

        public static string ToPath(this IEnumerable<string> parts)
        {
            return Path.Combine(parts.ToArray());
        }
    }
}