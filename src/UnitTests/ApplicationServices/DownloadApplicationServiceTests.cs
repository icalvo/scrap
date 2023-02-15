using Moq;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
using Scrap.Domain.Resources;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit.ApplicationServices;

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
        var builder = new DownloadApplicationServiceMockBuilder(_output);
        builder.ResourceRepositoryMock.Setup(x => x.Type).Returns("FileSystemRepository");
        var jobDto = JobDtoBuilder.Build(ResourceType.DownloadLink);
        builder.JobFactoryMock.SetupFactory(new Job(jobDto));
        var service = builder.Build();

        await service.DownloadAsync(
            jobDto,
            new Uri("https://example.com/a"),
            7,
            new Uri("http://example.com/1.txt"),
            8);


        builder.ResourceRepositoryMock.Verify(
            x => x.UpsertAsync(
                It.Is<ResourceInfo>(y => y.ResourceUrl == new Uri("http://example.com/1.txt")),
                It.IsAny<Stream>()),
            Times.Once);
    }
}
