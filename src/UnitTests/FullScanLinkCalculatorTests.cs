using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Scrap.Domain;
using Scrap.Domain.Pages;
using SharpX;
using Xunit;

namespace Scrap.Tests.Unit;

public class FullScanLinkCalculatorTests
{
    [Fact]
    public async Task CalculateLinks()
    {
        var mock = new Mock<IVisitedPageRepository>(MockBehavior.Strict);
        mock.Setup(x => x.ExistsAsync(new Uri("https://example.com/1.txt"))).ReturnsAsync(false);

        var lc = new FullScanLinkCalculator(Mock.Of<ILogger<FullScanLinkCalculator>>());
        XPath linkXPath = "//a/@href";
        var pageMock = new PageMock("https://example.com/a").PageLinks(
            linkXPath,
            "https://example.com/1.txt",
            "https://example.com/2.txt");

        (await lc.CalculateLinks(pageMock, linkXPath.ToJust()).ToArrayAsync()).Should().BeEquivalentTo(
            new[] { new Uri("https://example.com/1.txt"), new Uri("https://example.com/2.txt") });
    }
}
