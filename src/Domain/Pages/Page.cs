using System.Xml.XPath;
using Microsoft.Extensions.Logging;

namespace Scrap.Domain.Pages;

public class Page: IPage
{
    private readonly IPageRetriever _pageRetriever;
    private readonly Uri _baseUri;
    private readonly ILogger<Page> _logger;
    private readonly XPathNavigator _navigator;

    public Page(Uri uri, IXPathNavigable document, IPageRetriever pageRetriever, ILogger<Page> logger)
    {
        _pageRetriever = pageRetriever;
        _logger = logger;
        Uri = uri;
        Document = document;
        _navigator = document.CreateNavigator() ?? throw new ArgumentException("Cannot build an XPathNavigator from this doc", nameof(document));
        var portSuffix = uri.IsDefaultPort ? "" : $":{uri.Port}";
        _baseUri = new Uri($"{uri.Scheme}://{uri.Host}{portSuffix}");
    }

    public Uri Uri { get; }
    public IXPathNavigable Document { get; }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Page)obj);
    }

    public bool Equals(IPage? other)
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
                .Select(url => new Uri(_baseUri, url));
    }

    public Uri? Link(XPath xPath)
    {
        return Links(xPath).FirstOrDefault();
    }

    public string Concat(XPath xPath)
    {
        return string.Join("", Contents(xPath));
    }

    public string? ContentOrNull(XPath xPath)
    {
        return Contents(xPath).FirstOrDefault();
    }

    public IEnumerable<string> Contents(XPath xPath)
    {
        var nodesEnumerable = ToEnumerable(_navigator.Select(xPath));
        var results = xPath.IsHtml
            ? nodesEnumerable.Select(x => x.InnerXml)
            : nodesEnumerable.Select(x => x.Value);
        var resultsArray = results
            .RemoveNulls()
            .Where(url => !string.IsNullOrEmpty(url))
            .ToArray();
        LogXPathEval(resultsArray, xPath);
        return resultsArray;
    }

    private static IEnumerable<XPathNavigator> ToEnumerable(XPathNodeIterator iterator)
    {
        while (iterator.MoveNext())
        {
            if (iterator.Current != null)
            {
                yield return iterator.Current;
            }
        }
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

    public async Task<IPage?> LinkedDoc(string xPath)
    {
        var link = Link(xPath);
        if (link == null)
        {
            return null;
        }

        try
        {
            var linkedPage = await _pageRetriever.GetPageAsync(link);
            return new Page(link, linkedPage.Document, _pageRetriever, _logger);            
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"An error happened when getting a linked doc from XPath {xPath}",
                ex);
        }
    }

    private void LogXPathEval(string?[] result, XPath xPath)
    {
        const int maxElementsToDisplay = 3;
        const int maxCharsPerElement = 15;
        string?[] elementsToDisplay = result;
        string suffix = "";
        if (result.Length > maxElementsToDisplay)
        {
            elementsToDisplay = result[..maxElementsToDisplay];
            suffix = ",... (" + (result.Length - maxElementsToDisplay) + " more)";
        }

        string output = string.Join(",", elementsToDisplay.Select(x => x == null ? "" : x.Length <= maxCharsPerElement ? x : x[..(maxCharsPerElement - 3)] + "...")) + suffix;
        _logger.LogTrace("Eval XPath {XPath} => [{Result}]", xPath, output);
    }
}
