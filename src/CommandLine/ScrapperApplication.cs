using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CLAP;
using HtmlAgilityPack;

namespace Scrap.CommandLine
{
    public class ScrapperApplication
    {
        [Verb(IsDefault = true)]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static async Task Scrap(
            string rootUrl,
            string adjacencyXPath,
            string adjacencyAttribute,
            string resourceXPath,
            string resourceAttribute,
            string destinationRootFolder,
            string[] destinationFolderPattern,
            string[] destinationFileNamePattern)
        {
            var rootUri = new Uri(rootUrl);

            var baseUrl = new Uri(rootUri.Scheme + "://" + rootUri.Host);

            var web = new HtmlWeb();

            Page GetDocument(Uri uri)
            {
                while (true)
                {
                    try
                    {
                        Thread.Sleep(1000);
                        Console.WriteLine("GET {0}", uri);
                        return new Page(uri, web.Load(uri.AbsoluteUri));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                }
            }

            IEnumerable<Uri> AdjacencyFunction(Page page)
            {
                if (page == null) throw new ArgumentNullException(nameof(page));
                if (page.Document == null) throw new ArgumentNullException(nameof(page));
                if (page.Document.DocumentNode == null) throw new ArgumentNullException(nameof(page));
                return
                    page.Document
                        .DocumentNode
                        .SelectNodesBetter(adjacencyXPath)
                        .Select(node => node.Attributes?[adjacencyAttribute]?.Value)
                        .Where(url => !string.IsNullOrEmpty(url))
                        .Select(url => new Uri(baseUrl, url));
            }

            var pages = GraphSearch.DepthFirstSearch(rootUri, GetDocument, AdjacencyFunction);

            foreach (var page in pages)
            {
                var pageSegments = page.Uri.Segments.Select(segment => segment.Replace("/", "")).ToArray();
                var resources = page.Document
                    .DocumentNode
                    .SelectNodesBetter(resourceXPath)
                    .Select(node => node.Attributes[resourceAttribute].Value)
                    .Select(url => new Uri(baseUrl, url))
                    .ToArray();
                if (!resources.Any()) continue;
                foreach (var resource in resources)
                {
                    await ProcessResource(resource, destinationRootFolder, destinationFolderPattern, destinationFileNamePattern, pageSegments);
                }
            }
        }

        private static async Task ProcessResource(
            Uri resource,
            string destinationRootFolder,
            string[] destinationFolderPattern,
            string[] destinationFileNamePattern,
            string[] pageSegments)
        {
            var resourceSegments = resource.Segments.Select(segment => segment.Replace("/", "")).ToArray();
            var resourceExtension = Path.GetExtension(resource.Segments.Last());
            var data = new DestinationData(destinationRootFolder, pageSegments, resourceSegments,
                resourceExtension);

            var destinationFolderParts = data.Parse(destinationFolderPattern);
            var destinationFileNameParts = data.Parse(destinationFileNamePattern);
            var destinationFileName = string.Join("", destinationFileNameParts);
            var destinationPathParts = destinationFolderParts.Concat(new[] { destinationFileName });
            var destinationPath = Path.Combine(destinationPathParts.ToArray());

            Console.Write("GET {0} -> {1}", resource, destinationPath);
            var directoryName = Path.GetDirectoryName(destinationPath);
            if (directoryName != null)
            {
                Directory.CreateDirectory(directoryName);
                if (!File.Exists(destinationPath))
                {
                    await HttpHelper.DownloadFileAsync(resource, destinationPath);
                    Console.WriteLine(" OK!");
                }
                else
                {
                    Console.WriteLine(" Already there!");
                }
            }
        }
    }
}