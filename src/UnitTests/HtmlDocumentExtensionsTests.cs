using FluentAssertions;
using HtmlAgilityPack;
using Scrap.Domain;
using Xunit;

namespace Scrap.Tests;

public class HtmlDocumentExtensionsTests
{
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
    public void ContentsCases(string xPath, string[] expected)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(@"<html>
<body>
    <div id=""downloadCount"" data=""expectedData"">t1<div>t2a<span>t2b</span><span>t2c</span></div>t3</div>
    <a href=""link1""></a>
    <a href=""link2""><span>t4</span></a>
</body>
</html>");

        doc.Contents(xPath).Should().BeEquivalentTo(expected);
    }
        
}
