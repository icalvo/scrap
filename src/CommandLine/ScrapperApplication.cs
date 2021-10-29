using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CLAP;
using CLAP.Interception;
using HtmlAgilityPack;

namespace Scrap.CommandLine
{
    public class ScrapperApplication
    {
        private static readonly Func<Uri, Func<Uri, Page>, Func<Page, IEnumerable<Uri>>, IEnumerable<Page>> SearchFunc;
        private static readonly IPageRetriever PageRetriever;

        static ScrapperApplication()
        {
            SearchFunc = GraphSearch.DepthFirstSearch;
            PageRetriever = new CachedPageRetriever();
        }
        
        [Verb(IsDefault = true)]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static async Task Scrap(
            string rootUrl,
            string adjacencyXPath,
            string adjacencyAttribute,
            string resourceXPath,
            string resourceAttribute,
            string destinationRootFolder,
            string destinationExpression,
            bool whatIf = true)
        {
            PrintArguments(rootUrl, adjacencyXPath, adjacencyAttribute, resourceXPath, resourceAttribute, destinationRootFolder, destinationExpression);

            var rootUri = new Uri(rootUrl);
            var baseUrl = new Uri(rootUri.Scheme + "://" + rootUri.Host);

            Console.WriteLine("Compiling destination expression...");
            var destinationProvider = DestinationProvider.Create(destinationExpression, PageRetriever, baseUrl);

            IEnumerable<Uri> AdjacencyFunction(Page page)
            {
                if (page == null) throw new ArgumentNullException(nameof(page));
                if (page.Document == null) throw new ArgumentNullException(nameof(page));
                if (page.Document.DocumentNode == null) throw new ArgumentNullException(nameof(page));
                return
                    page.Document.GetLinks(adjacencyXPath, adjacencyAttribute, baseUrl);
            }

            var pages = SearchFunc(rootUri, PageRetriever.GetPage, AdjacencyFunction);

            foreach (var page in pages)
            {
                var resources = page.Document
                    .DocumentNode
                    .SelectNodesBetter(resourceXPath)
                    .Select(node => node.Attributes[resourceAttribute].Value)
                    .Select(url => new Uri(baseUrl, url))
                    .ToArray();
                if (!resources.Any())
                {
                    Console.WriteLine("No resources here matching " + resourceXPath);
                    continue;
                }

                foreach (var resource in resources)
                {
                    await ProcessResourceAsync(destinationProvider, resource, destinationRootFolder, page.Uri, page.Document, whatIf);
                }
            }
            
            Console.WriteLine("Finished!");
        }

        private static void PrintArguments(string rootUrl, string adjacencyXPath, string adjacencyAttribute,
            string resourceXPath, string resourceAttribute, string destinationRootFolder, string destinationExpression)
        {
            Console.WriteLine(nameof(rootUrl) + ": " + rootUrl);
            Console.WriteLine(nameof(adjacencyXPath) + ": " + adjacencyXPath);
            Console.WriteLine(nameof(adjacencyAttribute) + ": " + adjacencyAttribute);
            Console.WriteLine(nameof(resourceXPath) + ": " + resourceXPath);
            Console.WriteLine(nameof(resourceAttribute) + ": " + resourceAttribute);
            Console.WriteLine(nameof(destinationRootFolder) + ": " + destinationRootFolder);
            Console.WriteLine(nameof(destinationExpression) + ": " + destinationExpression);
        }

        private static async Task ProcessResourceAsync(
            DestinationProvider destinationProvider,
            Uri resourceUrl,
            string destinationRootFolder,
            Uri pageUrl,
            HtmlDocument pageDoc,
            bool whatIf)
        {
            var destinationPath = destinationProvider.GetDestination(
                resourceUrl,
                destinationRootFolder,
                pageUrl,
                pageDoc
            );

            Console.WriteLine("GET {0}", resourceUrl);
            Console.WriteLine("-> {0}", destinationPath);
            try
            {
                var directoryName = Path.GetDirectoryName(destinationPath);
                if (directoryName != null)
                {
                    Directory.CreateDirectory(directoryName);
                    if (!File.Exists(destinationPath))
                    {
                        if (!whatIf) {
                            await HttpHelper.DownloadFileAsync(resourceUrl, destinationPath);
                        }
                        Console.WriteLine(" OK!");
                    }
                    else
                    {
                        Console.WriteLine(" Already there!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        [PostVerbExecution]
        public static void After(PostVerbExecutionContext context)
        {
            Console.WriteLine(context.Exception);
        }        
    }
}