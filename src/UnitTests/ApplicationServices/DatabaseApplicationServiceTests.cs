using Moq;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Pages;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit.ApplicationServices;

public class DatabaseApplicationServiceTests
{
    private readonly ITestOutputHelper _output;

    public DatabaseApplicationServiceTests(ITestOutputHelper output)
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
        var service = builder.BuildDatabaseApplicationService();

        _ = await service.SearchAsync("x");

        builder.PageMarkerRepositoryMock.Verify(x => x.SearchAsync("x"), Times.Once);
    }    
}
