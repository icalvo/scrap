using FluentAssertions;
using Moq;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Resources;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit.ApplicationServices;

public class ScrapTextsServiceTests
{
    private readonly ITestOutputHelper _output;

    public ScrapTextsServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ScrapTextAsync()
    {
        var builder = new MockBuilder(
            _output,
            new PageMock("https://example.com/a")
                .Contents(MockBuilder.ResourceXPath, "qwer", "asdf"),
            new PageMock(
                    "https://example.com/b")
                .Contents(MockBuilder.ResourceXPath, "zxcv", "yuio"));
        builder.ResourceRepositoryMock.Setup(x => x.Type).Returns("FileSystemRepository");
        var jobDto = builder.BuildJobDto(ResourceType.Text);
        var service = builder.BuildScrapTextsService(jobDto);

        await service.ScrapTextAsync(jobDto);

        builder.ResourceRepositoryMock.Verify(
            x => x.UpsertAsync(It.IsAny<ResourceInfo>(), It.IsAny<Stream>()),
            Times.Exactly(4));

        builder.PageMarkerRepositoryMock.Verify(
            x => x.ExistsAsync(It.IsAny<Uri>()),
            Times.Never);
        builder.PageMarkerRepositoryMock.Verify(
            x => x.UpsertAsync(new Uri("https://example.com/a")),
            Times.Once);
        builder.PageMarkerRepositoryMock.Verify(
            x => x.UpsertAsync(new Uri("https://example.com/b")),
            Times.Once);
    }


    [Fact]
    public async Task? ScrapTextAsync_DownloadJob_Throws()
    {
        var builder = new MockBuilder(_output);
        var jobDto = builder.BuildJobDto(ResourceType.DownloadLink);
        builder.ResourceRepositoryMock.Setup(x => x.Type).Returns("FileSystemRepository");
        var service = builder.BuildScrapTextsService(jobDto);

        var action = () => service.ScrapTextAsync(jobDto);
        await action.Should().ThrowAsync<InvalidOperationException>();
    }
}
