using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Scrap.Pages;
using Xunit;

namespace Scrap.Tests;

public class LinkCalculatorTests
{
    [Fact]
    public async Task CalculateLinks_LinkIsNotMarked()
    {
        var mock = new Mock<IPageMarkerRepository>(MockBehavior.Strict);
        mock.Setup(x => x.ExistsAsync(new Uri("http://example.com/1.txt"))).ReturnsAsync(false);
        mock.Setup(x => x.ExistsAsync(new Uri("http://example.com/2.txt"))).ReturnsAsync(false);

        var lc = new LinkCalculator(Mock.Of<ILogger<LinkCalculator>>(), mock.Object);
        var linkXPath = "//a/@href";
        var pageMock = TestTools.PageMock("https://example.com/a", linkXPath, "http://example.com/1.txt",
            "http://example.com/2.txt");

        (await lc.CalculateLinks(pageMock, linkXPath).ToArrayAsync()).Should().BeEquivalentTo(new[]
        {
            new Uri("http://example.com/1.txt"),
            new Uri("http://example.com/2.txt")
        });
    }

    [Fact]
    public async Task CalculateLinks_LinkIsMarked()
    {
        var mock = new Mock<IPageMarkerRepository>(MockBehavior.Strict);
        mock.Setup(x => x.ExistsAsync(new Uri("http://example.com/1.txt"))).ReturnsAsync(true);
        mock.Setup(x => x.ExistsAsync(new Uri("http://example.com/2.txt"))).ReturnsAsync(false);

        var lc = new LinkCalculator(Mock.Of<ILogger<LinkCalculator>>(), mock.Object);
        var linkXPath = "//a/@href";
        var pageMock = TestTools.PageMock("https://example.com/a", linkXPath, "http://example.com/1.txt",
            "http://example.com/2.txt");

        (await lc.CalculateLinks(pageMock, linkXPath).ToArrayAsync()).Should().BeEquivalentTo(new[]
        {
            new Uri("http://example.com/2.txt"),
        });
    }    
}