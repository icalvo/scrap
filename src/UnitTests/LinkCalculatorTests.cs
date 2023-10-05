using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Scrap.Domain.Pages;
using Xunit;

namespace Scrap.Tests.Unit;

public class LinkCalculatorTests
{
    [Fact]
    public async Task CalculateLinks_LinkIsNotMarked()
    {
        var mock = new Mock<IVisitedPageRepository>(MockBehavior.Strict);
        mock.Setup(x => x.ExistsAsync(new Uri("https://example.com/1.txt"))).ReturnsAsync(false);
        mock.Setup(x => x.ExistsAsync(new Uri("https://example.com/2.txt"))).ReturnsAsync(false);

        var lc = new LinkCalculator(Mock.Of<ILogger<LinkCalculator>>(), mock.Object);
        var linkXPath = "//a/@href";
        var pageMock = new PageMock("https://example.com/a").PageLinks(
            linkXPath,
            "https://example.com/1.txt",
            "https://example.com/2.txt");

        (await lc.CalculateLinks(pageMock, linkXPath).ToArrayAsync()).Should().BeEquivalentTo(
            new[] { new Uri("https://example.com/1.txt"), new Uri("https://example.com/2.txt") });
    }

    [Fact]
    public async Task CalculateLinks_LinkIsMarked()
    {
        var mock = new Mock<IVisitedPageRepository>(MockBehavior.Strict);
        mock.Setup(x => x.ExistsAsync(new Uri("https://example.com/1.txt"))).ReturnsAsync(true);
        mock.Setup(x => x.ExistsAsync(new Uri("https://example.com/2.txt"))).ReturnsAsync(false);

        var lc = new LinkCalculator(Mock.Of<ILogger<LinkCalculator>>(), mock.Object);
        var linkXPath = "//a/@href";
        var pageMock = new PageMock("https://example.com/a").PageLinks(
            linkXPath,
            "https://example.com/1.txt",
            "https://example.com/2.txt");

        (await lc.CalculateLinks(pageMock, linkXPath).ToArrayAsync()).Should()
            .BeEquivalentTo(new[] { new Uri("https://example.com/2.txt") });
    }
}
