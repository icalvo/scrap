using FluentAssertions;
using HtmlAgilityPack;
using Xunit;

namespace Scrap.Tests
{
    public class HtmlDocumentExtensionsTests
    {
        [Fact]
        public void ContentsCases()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(@"<html>
<body>
    <div id=""downloadCount"" data=""expectedData"">t1<div>t2a<span>t2b</span><span>t2c</span></div>t3</div>
    <a href=""link1""></a>
    <a href=""link2""><span>t4</span></a>
</body>
</html>");

            // Attribute value
            doc.Contents("//*[@id='downloadCount']/@data").Should().BeEquivalentTo("expectedData");
            // For all selected nodes, inner recursive texts, concatenated
            doc.Contents("//*[@id='downloadCount']").Should().BeEquivalentTo("t1t2at2bt2ct3");
            // For all selected nodes, one element with the inner non-recursive text
            doc.Contents("//*[@id='downloadCount']/text()").Should().BeEquivalentTo("t1", "t3");
            // Concatenated inner recursive text of immediate children
            doc.Contents("//*[@id='downloadCount']/*").Should().BeEquivalentTo("t2at2bt2c");
            // For each immediate children: Split inner text, non recursive
            doc.Contents("//*[@id='downloadCount']/*/text()").Should().BeEquivalentTo("t2a");
            // For each descendant: Concatenated inner text, recursive
            doc.Contents("//*[@id='downloadCount']//*").Should().BeEquivalentTo("t2at2bt2c", "t2b", "t2c");
            // For each descendant: Split inner text, non recursive
            doc.Contents("//*[@id='downloadCount']//*/text()").Should().BeEquivalentTo("t2a", "t2b", "t2c");
            // node() iterates all direct children nodes (text nodes and tag nodes).
            doc.Contents("//*[@id='downloadCount']/node()").Should().BeEquivalentTo("t1", "t2at2bt2c", "t3");
            doc.Contents("//a/@href").Should().BeEquivalentTo("link1", "link2");
            doc.Contents("html://*[@id='downloadCount'] | //a[2]").Should().BeEquivalentTo("t1<div>t2a<span>t2b</span><span>t2c</span></div>t3", "<span>t4</span>");
        }
        
    }
}
