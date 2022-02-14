using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Scrap.Pages;
using Xunit;

namespace Scrap.Tests;

public class FullScanLinkCalculatorTests
{
    [Fact]
    public async Task CalculateLinks()
    {
        var mock = new Mock<IPageMarkerRepository>(MockBehavior.Strict);
        mock.Setup(x => x.ExistsAsync(new Uri("http://example.com/1.txt"))).ReturnsAsync(false);

        var lc = new FullScanLinkCalculator(Mock.Of<ILogger<FullScanLinkCalculator>>());
        var linkXPath = "//a/@href";
        var pageMock = TestTools.PageMock("https://example.com/a", linkXPath,
            "http://example.com/1.txt",
            "http://example.com/2.txt");

        (await lc.CalculateLinks(pageMock, linkXPath).ToArrayAsync()).Should().BeEquivalentTo(new[]
        {
            new Uri("http://example.com/1.txt"),
            new Uri("http://example.com/2.txt")
        });
    }
}