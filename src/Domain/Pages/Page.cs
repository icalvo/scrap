using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.XPath;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace Scrap.Pages;

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
        
    public  IEnumerable<Uri> Links(XPath xPath)
    {
        return
            Contents(xPath)
                .Where(url => !string.IsNullOrEmpty(url))
                .Select(url => new Uri(_baseUri, url!));
    }

    public Uri? Link(XPath xPath)
    {
        return Links(xPath).FirstOrDefault();
    }

    public string Concat(XPathExpression xPath)
    {
        return string.Join("", Contents(xPath));
    }

    public string? ContentOrNull(XPath xPath)
    {
        return Contents(xPath).FirstOrDefault();
    }

    public IEnumerable<string?> Contents(XPath xPath)
    {
        var result = Document.Contents(xPath).ToArray();

        string?[] elementsToDisplay = result;
        string suffix = "";
        const int maxElementsToDisplay = 3;
        const int maxCharsPerElement = 15;
        if (result.Length > maxElementsToDisplay)
        {
            elementsToDisplay = result[..maxElementsToDisplay];
            suffix = ",... (" + (result.Length - maxElementsToDisplay) + " more)";
        }

        string output = string.Join(",", elementsToDisplay.Select(x => x == null ? "" : x.Length <= maxCharsPerElement ? x : x[..(maxCharsPerElement - 3)] + "...")) + suffix;
        _logger.LogDebug("Eval XPath {XPath} => [{Result}]", xPath, output);
            
        return result;
    }
        
    public string Content(XPath xPath)
    {
        var result = Contents(xPath).FirstOrDefault();
        if (string.IsNullOrEmpty(result))
        {
            throw new ArgumentException($"XPath {xPath} has no content.", nameof(xPath));
        }

        return result;
    }

    public async Task<Page?> LinkedDoc(string xPath)
    {
        var link = Link(xPath);
        try
        {
            return await Doc(link);
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"An error happened when getting a linked doc from XPath {xPath}",
                ex);
        }
    }

    private async Task<Page?> Doc(Uri? link)
    {
        Debug.Assert(_pageRetriever != null, nameof(_pageRetriever) + " != null");

        return link == null ? null : new Page(link, (await _pageRetriever.GetPageAsync(link)).Document, _pageRetriever, _logger);
    }        
}
