using Moq;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Pages;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit.ApplicationServices;

public class VisitedPagesApplicationServiceTests
{
    private readonly ITestOutputHelper _output;

    public VisitedPagesApplicationServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task SearchAsync()
    {
        var builder = new MockBuilder(_output);
        builder.PageMarkerRepositoryMock.Setup(x => x.SearchAsync(It.IsAny<string>())).ReturnsAsync(new []
        {
            new PageMarker("https://x.com"),
            new PageMarker("https://x.com/2")
        });
        var jobDto = builder.BuildJobDto(ResourceType.DownloadLink);
        var service = builder.BuildVisitedPagesApplicationService();

        _ = await service.SearchAsync("x");

        builder.PageMarkerRepositoryMock.Verify(x => x.SearchAsync("x"), Times.Once);
    }
    
    [Fact]
    public async Task MarkVisitedPageAsync()
    {
        var builder = new MockBuilder(_output);
        var jobDto = builder.BuildJobDto(ResourceType.DownloadLink);
        var service = builder.BuildVisitedPagesApplicationService();

        await service.MarkVisitedPageAsync(new Uri("https://example.com/a"));

        builder.PageMarkerRepositoryMock.Verify(
            x => x.ExistsAsync(It.IsAny<Uri>()),
            Times.Never);
        builder.PageMarkerRepositoryMock.Verify(
            x => x.UpsertAsync(It.IsAny<Uri>()),
            Times.Once);
        builder.PageMarkerRepositoryMock.Verify(
            x => x.UpsertAsync(new Uri("https://example.com/a")),
            Times.Once);
    }
}
