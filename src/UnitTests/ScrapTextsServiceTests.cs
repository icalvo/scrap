using FluentAssertions;
using Moq;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Resources;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit;

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
        var builder = new ScrapTextsServiceMockBuilder(_output);
        builder.SetupTraversal(
            new PageMock("https://example.com/a").Contents(JobBuilder.ResourceXPath, "qwer", "asdf"),
            new PageMock("https://example.com/b").Contents(JobBuilder.ResourceXPath, "zxcv", "yuio"));
        builder.ResourceRepositoryMock.Setup(x => x.Type).Returns("FileSystemRepository");
        var job = JobBuilder.BuildScrap(ResourceType.Text);
        var service = builder.Build();

        await service.ScrapTextAsync(job);

        builder.ResourceRepositoryMock.Verify(
            x => x.UpsertAsync(It.IsAny<ResourceInfo>(), It.IsAny<Stream>()),
            Times.Exactly(4));

        builder.VisitedPageRepositoryMock.Verify(x => x.ExistsAsync(It.IsAny<Uri>()), Times.Never);
        builder.VisitedPageRepositoryMock.Verify(x => x.UpsertAsync(new Uri("https://example.com/a")), Times.Once);
        builder.VisitedPageRepositoryMock.Verify(x => x.UpsertAsync(new Uri("https://example.com/b")), Times.Once);
    }
}
