using System.Xml.XPath;
using Moq;
using Scrap.Domain;
using Scrap.Domain.Pages;

namespace Scrap.Tests.Unit;

public class PageMock : IPage
{
    private readonly Mock<IPage> _pageMock;

    public PageMock(string uri)
    {
        _pageMock = new Mock<IPage>(MockBehavior.Strict);
        _pageMock.Setup(x => x.Document)
            .Returns(Mock.Of<IXPathNavigable>(x => x.CreateNavigator() == Mock.Of<XPathNavigator>()));
        _pageMock.Setup(x => x.Uri).Returns(new Uri(uri));
    }

    private IPage Page => _pageMock.Object;

    bool IEquatable<IPage>.Equals(IPage? other) => Page.Equals(other);

    Uri IPage.Uri => Page.Uri;

    IXPathNavigable IPage.Document => Page.Document;

    IEnumerable<Uri> IPage.Links(XPath xPath) => Page.Links(xPath);

    Uri? IPage.Link(XPath xPath) => Page.Link(xPath);

    string IPage.Concat(XPath xPath) => Page.Concat(xPath);

    IEnumerable<string?> IPage.Contents(XPath xPath) => Page.Contents(xPath);

    string IPage.Content(XPath xPath) => Page.Content(xPath);

    Task<IPage?> IPage.LinkedDoc(string xPath) => Page.LinkedDoc(xPath);

    string? IPage.ContentOrNull(XPath xPath) => Page.ContentOrNull(xPath);

    public Task<IPage> ReloadAsync() => Task.FromResult((IPage)this);

    public PageMock PageLinks(XPath linkXPath, params string[] linkResults)
    {
        _pageMock.Setup(x => x.Links(linkXPath)).Returns(linkResults.Select(x => new Uri(x)));

        return this;
    }

    public PageMock ResourceLinks(XPath resourceXPath, params string[] resourceResults)
    {
        _pageMock.Setup(x => x.Links(resourceXPath)).Returns(resourceResults.Select(x => new Uri(x)));

        return this;
    }

    public PageMock Contents(XPath contentsXPath, params string[] contentsResults)
    {
        _pageMock.Setup(x => x.Contents(contentsXPath)).Returns(contentsResults);

        return this;
    }
}
