using Moq;
using Scrap.Domain.JobDefinitions;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit.ApplicationServices;

public class MarkVisitedApplicationServiceTests
{
    private readonly ITestOutputHelper _output;

    public MarkVisitedApplicationServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task MarkVisitedPageAsync()
    {
        var builder = new MockBuilder(_output);
        var jobDto = builder.BuildJobDto(ResourceType.DownloadLink);
        var service = builder.BuildMarkVisitedApplicationService(jobDto);

        await service.MarkVisitedPageAsync(jobDto, new Uri("https://example.com/a"));

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
