using Moq;
using Scrap.Domain.Resources;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests;

public class DownloadApplicationServiceTests
{
    private readonly ITestOutputHelper _output;

    public DownloadApplicationServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task DownloadAsync()
    {
        var builder = new MockBuilder(_output);
        var jobDto = builder.BuildJobDto();
        var service = builder.BuildDownloadApplicationService(jobDto);

        await service.DownloadAsync(
            jobDto,
            new Uri("https://example.com/a"), 7,
            new Uri("http://example.com/1.txt"), 8);


        builder.ResourceRepositoryMock.Verify(
            x => x.UpsertAsync(It.Is<ResourceInfo>(y => y.ResourceUrl == new Uri("http://example.com/1.txt")),
                It.IsAny<Stream>()),
            Times.Once);
    }
}
