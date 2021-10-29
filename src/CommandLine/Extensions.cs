using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;

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

        public static IEnumerable<Uri> GetLinks(this HtmlDocument? document, string adjacencyXPath, string adjacencyAttribute, Uri baseUrl)
        {
            if (document == null) return Enumerable.Empty<Uri>();
            if (document.DocumentNode == null) throw new ArgumentNullException(nameof(document));
            return
                document.GetAttributes(adjacencyXPath, adjacencyAttribute)
                    .Where(url => !string.IsNullOrEmpty(url))
                    .Select(url => new Uri(baseUrl, url));
        }

        public static Uri? GetLink(this HtmlDocument? document, string adjacencyXPath, string adjacencyAttribute, Uri baseUrl)
        {
            if (document == null) return null;
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (document.DocumentNode == null) throw new ArgumentNullException(nameof(document));
            return
                document.GetLinks(adjacencyXPath, adjacencyAttribute, baseUrl).FirstOrDefault();
        }

        public static IEnumerable<string?> GetContents(this HtmlDocument? document, string adjacencyXPath)
        {
            if (document == null) return Enumerable.Empty<string?>();
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (document.DocumentNode == null) throw new ArgumentNullException(nameof(document));
            return
                document
                    .DocumentNode
                    .SelectNodesBetter(adjacencyXPath)
                    .Select(node => node.InnerText);
        }

        public static string? GetContent(this HtmlDocument? document, string adjacencyXPath)
        {
            if (document == null) return null;
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (document.DocumentNode == null) throw new ArgumentNullException(nameof(document));
            return
                string.Join("", document.GetContents(adjacencyXPath));
        }

        public static IEnumerable<string?> GetAttributes(this HtmlDocument? document, string adjacencyXPath, string adjacencyAttribute)
        {
            if (document == null) return Enumerable.Empty<string?>();
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (document.DocumentNode == null) throw new ArgumentNullException(nameof(document));
            return
                document
                    .DocumentNode
                    .SelectNodesBetter(adjacencyXPath)
                    .Select(node => node.Attributes?[adjacencyAttribute]?.Value);
        }
        
        public static string? GetAttribute(this HtmlDocument? document, string adjacencyXPath, string adjacencyAttribute)
        {
            if (document == null) return null;
            if (document.DocumentNode == null) throw new ArgumentNullException(nameof(document));
            return
                document
                    .DocumentNode
                    .SelectNodesBetter(adjacencyXPath)
                    .Select(node => node.Attributes?[adjacencyAttribute]?.Value)
                    .FirstOrDefault();
        }

        public static string Combine(this IEnumerable<string> parts)
        {
            return Path.Combine(parts.ToArray());
        }
    }
}