using FluentAssertions;
using Moq;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Resources;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests;

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
            TestTools.PageMock(
                "https://example.com/a",
                MockBuilder.LinkXPath, new[] { "https://example.com/b" },
                MockBuilder.ResourceXPath, new[] { "https://example.com/1.txt", "https://example.com/2.txt" },
                MockBuilder.ResourceXPath, new[] { "qwer", "asdf" }),
            TestTools.PageMock(
                "https://example.com/b",
                MockBuilder.LinkXPath, new[] { "https://example.com/a" },
                MockBuilder.ResourceXPath, new[] { "https://example.com/3.txt", "https://example.com/4.txt" },
                MockBuilder.ResourceXPath, new[] { "zxcv", "yuio" }));
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
        var builder = new MockBuilder(
            _output,
            TestTools.PageMock(
                "https://example.com/a",
                MockBuilder.LinkXPath, new[] { "https://example.com/b" },
                MockBuilder.ResourceXPath, new[] { "https://example.com/1.txt", "https://example.com/2.txt" },
                MockBuilder.ResourceXPath, new[] { "qwer", "asdf" }),
            TestTools.PageMock(
                "https://example.com/b",
                MockBuilder.LinkXPath, new[] { "https://example.com/a" },
                MockBuilder.ResourceXPath, new[] { "https://example.com/3.txt", "https://example.com/4.txt" },
                MockBuilder.ResourceXPath, new[] { "zxcv", "yuio" }));
        var jobDto = builder.BuildJobDto(ResourceType.DownloadLink);
        var service = builder.BuildScrapTextsService(jobDto);

        var action = () => service.ScrapTextAsync(jobDto);
        await action.Should().ThrowAsync<Exception>();
    }
}
