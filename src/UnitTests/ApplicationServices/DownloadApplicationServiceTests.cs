using Moq;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Resources;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.ApplicationServices;

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
        builder.ResourceRepositoryMock.Setup(x => x.Type).Returns("FileSystemRepository");
        var jobDto = builder.BuildJobDto(ResourceType.DownloadLink);
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
