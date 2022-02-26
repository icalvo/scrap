using System.Xml.XPath;
using FluentAssertions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Moq;
using Scrap.Domain;
using Scrap.Domain.Pages;
using Xunit;

namespace Scrap.Tests;

public class PageTests
{
    [Fact]
    public void Links_ResolvesRelativeLinks()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(@"<html>
<body>
    <div id=""downloadCount"" data=""expectedData"">t1<div>t2a<span>t2b</span><span>t2c</span></div>t3</div>
    <a href=""a""></a>
    <a href=""b""><span>t4</span></a>
</body>
</html>");

        var page = new Page(
            new Uri("https://example.com"),
            doc,
            Mock.Of<IPageRetriever>(),
            Mock.Of<ILogger<Page>>());

        var links = page.Links("//a/@href");

        links.Should().BeEquivalentTo(new[] {new Uri("https://example.com/a"), new Uri("https://example.com/b")});
    }

    [Fact]
    public void Links_AbsoluteLinks()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(@"<html>
<body>
    <div id=""downloadCount"" data=""expectedData"">t1<div>t2a<span>t2b</span><span>t2c</span></div>t3</div>
    <a href=""https://other.com/a""></a>
    <a href=""https://other.com/b""><span>t4</span></a>
</body>
</html>");

        var page = new Page(
            new Uri("https://example.com"),
            doc,
            Mock.Of<IPageRetriever>(),
            Mock.Of<ILogger<Page>>());

        var links = page.Links("//a/@href");

        links.Should().BeEquivalentTo(new[] {new Uri("https://other.com/a"), new Uri("https://other.com/b")});
    }

    [Fact]
    public void Links_RemovesEmptyLinks()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(@"<html>
<body>
    <div id=""downloadCount"" data=""expectedData"">t1<div>t2a<span>t2b</span><span>t2c</span></div>t3</div>
    <a href=""""></a>
    <a href=""https://other.com/b""><span>t4</span></a>
</body>
</html>");

        var page = new Page(
            new Uri("https://example.com"),
            doc,
            Mock.Of<IPageRetriever>(),
            Mock.Of<ILogger<Page>>());

        var links = page.Links("//a/@href");

        links.Should().BeEquivalentTo(new[] {new Uri("https://other.com/b")});
    }    

    [Fact]
    public void Link_AbsoluteLinks()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(@"<html>
<body>
    <div id=""downloadCount"" data=""expectedData"">t1<div>t2a<span>t2b</span><span>t2c</span></div>t3</div>
    <a href=""https://other.com/a""></a>
    <a href=""https://other.com/b""><span>t4</span></a>
</body>
</html>");

        var page = new Page(
            new Uri("https://example.com"),
            doc,
            Mock.Of<IPageRetriever>(),
            Mock.Of<ILogger<Page>>());

        var links = page.Link("//a/@href");

        links.Should().Be(new Uri("https://other.com/a"));
    }

    [Fact]
    public async Task LinkedDoc()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(@"<html>
<body>
    <div id=""downloadCount"" data=""expectedData"">t1<div>t2a<span>t2b</span><span>t2c</span></div>t3</div>
    <a href=""""></a>
    <a href=""https://other.com/a""><span>t4</span></a>
</body>
</html>");

        var expectedLinkedPage = new PageMock("https://other.com/a");
        var retrieverMock = new Mock<IPageRetriever>(MockBehavior.Strict);
        retrieverMock.Setup(x => x.GetPageAsync(new Uri("https://other.com/a")))
            .ReturnsAsync(expectedLinkedPage);

        var page = new Page(
            new Uri("https://example.com"),
            doc,
            retrieverMock.Object,
            Mock.Of<ILogger<Page>>());

        var linkedPage = await page.LinkedDoc("//a/@href");

        linkedPage?.Uri.Should().Be(((IPage)expectedLinkedPage).Uri);
    }

    [Theory]
    // Attribute value
    [InlineData("//*[@id='downloadCount']/@data", new[]{"expectedData"})]
    // For all selected nodes, inner recursive texts, concatenated
    [InlineData("//*[@id='downloadCount']", new[]{"t1t2at2bt2ct3"})]
    // For all selected nodes, one element with the inner non-recursive text
    [InlineData("//*[@id='downloadCount']/text()", new[]{"t1", "t3"})]
    // Concatenated inner recursive text of immediate children
    [InlineData("//*[@id='downloadCount']/*", new[]{"t2at2bt2c"})]
    // For each immediate children: Split inner text, non recursive
    [InlineData("//*[@id='downloadCount']//*/text()", new[]{"t2a", "t2b", "t2c"})]
    // node() iterates all direct children nodes (text nodes and tag nodes).
    [InlineData("//*[@id='downloadCount']/node()", new[]{"t1", "t2at2bt2c", "t3"})]
    [InlineData("//a/@href", new[]{"link1", "link2"})]
    [InlineData("html://*[@id='downloadCount'] | //a[2]", new[]{"t1<div>t2a<span>t2b</span><span>t2c</span></div>t3", "<span>t4</span>"})]
    public void Contents_XPathTests(string xPath, string[] expected)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(@"<html>
<body>
    <div id=""downloadCount"" data=""expectedData"">t1<div>t2a<span>t2b</span><span>t2c</span></div>t3</div>
    <a href=""link1""></a>
    <a href=""link2""><span>t4</span></a>
</body>
</html>");

        var page = new Page(new Uri("https://example.com"), doc, Mock.Of<IPageRetriever>(), Mock.Of<ILogger<Page>>());
        page.Contents(xPath).Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Content_NoMatch_Throws()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(@"<html>
<body>
    <div id=""downloadCount"" data=""expectedData"">t1<div>t2a<span>t2b</span><span>t2c</span></div>t3</div>
    <a href=""https://other.com/a""></a>
    <a href=""https://other.com/b""><span>t4</span></a>
</body>
</html>");

        var page = new Page(
            new Uri("https://example.com"),
            doc,
            Mock.Of<IPageRetriever>(),
            Mock.Of<ILogger<Page>>());

        var action = () => page.Content("//xxx");

        action.Should().Throw<ArgumentException>();
    }


    [Fact]
    public void ContentOrNull_NoMatch_ReturnsNull()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(@"<html>
<body>
    <div id=""downloadCount"" data=""expectedData"">t1<div>t2a<span>t2b</span><span>t2c</span></div>t3</div>
    <a href=""https://other.com/a""></a>
    <a href=""https://other.com/b""><span>t4</span></a>
</body>
</html>");

        var page = new Page(
            new Uri("https://example.com"),
            doc,
            Mock.Of<IPageRetriever>(),
            Mock.Of<ILogger<Page>>());

        var actual = page.ContentOrNull("//xxx");

        actual.Should().BeNull();
    }
}
