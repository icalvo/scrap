using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace Scrap.Pages
{
    public class Page: IEquatable<Page>
    {
        private readonly IPageRetriever _pageRetriever;
        private readonly Uri _baseUri;
        private readonly ILogger<Page> _logger;

        public Page(Uri uri, HtmlDocument document, IPageRetriever pageRetriever, ILogger<Page> logger)
        {
            _pageRetriever = pageRetriever;
            _logger = logger;
            Uri = uri;
            Document = document;
            _baseUri = new Uri(uri.Scheme + "://" + uri.Host);
        }

        public Uri Uri { get; }
        private HtmlDocument Document { get; }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Page)obj);
        }

        public bool Equals(Page? other)
        {
            return other != null && Uri.AbsoluteUri.Equals(other.Uri.AbsoluteUri);
        }

        public override int GetHashCode()
        {
            return Uri.AbsoluteUri.GetHashCode();
        }
        
        public IEnumerable<HtmlNode> SelectNodes(string xpath)
        {
            try
            {
                return Document.DocumentNode.SelectNodes(xpath) ?? Enumerable.Empty<HtmlNode>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error selecting {XPath}: {Message}", xpath, ex.Message);
            }

            return Enumerable.Empty<HtmlNode>();
        }
        
        public  IEnumerable<Uri> Links(string adjacencyXPath, string adjacencyAttribute)
        {
            return
                Attributes(adjacencyXPath, adjacencyAttribute)
                    .Where(url => !string.IsNullOrEmpty(url))
                    .Select(url => new Uri(_baseUri, url!));
        }

        public IEnumerable<Uri> Links(string adjacencyXPath)
        {
            return Links(adjacencyXPath, "href");
        }

        public Uri? Link(string adjacencyXPath, string adjacencyAttribute, Uri? baseUrl = null)
        {
            return Links(adjacencyXPath, adjacencyAttribute).FirstOrDefault();
        }

        public Uri? Link(string adjacencyXPath, Uri? baseUrl = null) 
        {
            return Links(adjacencyXPath, "href").FirstOrDefault();
        }

        public IEnumerable<string?> Texts(string xPath)
        {
            _logger.LogDebug("Get innerTexts from {XPath}", xPath);
            return
                SelectNodes(xPath)
                .Select(node => node.InnerText);
        }

        public string AllText(string adjacencyXPath)
        {
            return string.Join("", Texts(adjacencyXPath));
        }

        public string? Text(string adjacencyXPath)
        {
            return Texts(adjacencyXPath).FirstOrDefault();
        }

        public IEnumerable<string?> Attributes(string linksXPath, string linkAttribute)
        {
            _logger.LogDebug("Get {LinkAttribute} from {LinksXPath}", linkAttribute, linksXPath);
            return
                SelectNodes(linksXPath)
                .Select(node => node.Attributes?[linkAttribute]?.Value);
        }
        
        public string? Attribute(string xPath, string attribute)
        {
            return Attributes(xPath, attribute).FirstOrDefault();
        }

        public Task<Page?> LinkedDoc(string adjacencyXPath, Uri? baseUrl = null)
        {
            var link = Link(adjacencyXPath, baseUrl);
            return Doc(link);
        }

        private async Task<Page?> Doc(Uri? link)
        {
            Debug.Assert(_pageRetriever != null, nameof(_pageRetriever) + " != null");

            return link == null ? null : new Page(link, (await _pageRetriever.GetPageAsync(link)).Document, _pageRetriever, _logger);
        }        
    }
}
