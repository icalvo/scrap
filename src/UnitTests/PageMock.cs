using System.Xml.XPath;
using Moq;
using Scrap.Domain;
using Scrap.Domain.Pages;

namespace Scrap.Tests;

public class PageMock : IPage
{
    private readonly Mock<IPage> _pageMock;
    private IPage Page => _pageMock.Object;
    
    public PageMock(string uri)
    {
        _pageMock = new Mock<IPage>();
        _pageMock.Setup(x => x.Uri)
            .Returns(new Uri(uri));
    }

    public PageMock PageLinks(XPath linkXPath, params string[] linkResults)
    {
        _pageMock.Setup(x => x.Links(linkXPath))
            .Returns(linkResults.Select(x => new Uri(x)));

        return this;
    }

    public PageMock ResourceLinks(XPath resourceXPath, params string[] resourceResults)
    {
        _pageMock.Setup(x => x.Links(resourceXPath))
            .Returns(resourceResults.Select(x => new Uri(x)));

        return this;
    }

    public PageMock Contents(XPath contentsXPath, params string[] contentsResults)
    {
        _pageMock.Setup(x => x.Contents(contentsXPath))
            .Returns(contentsResults);

        return this;
    }

    bool IEquatable<IPage>.Equals(IPage? other)
    {
        return Page.Equals(other);
    }

    Uri IPage.Uri => Page.Uri;

    IXPathNavigable IPage.Document => Page.Document;

    IEnumerable<Uri> IPage.Links(XPath xPath)
    {
        return Page.Links(xPath);
    }

    Uri? IPage.Link(XPath xPath)
    {
        return Page.Link(xPath);
    }

    string IPage.Concat(XPath xPath)
    {
        return Page.Concat(xPath);
    }

    IEnumerable<string?> IPage.Contents(XPath xPath)
    {
        return Page.Contents(xPath);
    }

    string IPage.Content(XPath xPath)
    {
        return Page.Content(xPath);
    }

    Task<IPage?> IPage.LinkedDoc(string xPath)
    {
        return Page.LinkedDoc(xPath);
    }

    string? IPage.ContentOrNull(XPath xPath)
    {
        return Page.ContentOrNull(xPath);
    }
}
