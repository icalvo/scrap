using Moq;
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
        var builder = new VisitedPagesApplicationServiceMockBuilder();
        builder.VisitedPageRepositoryMock.Setup(x => x.SearchAsync(It.IsAny<string>())).Returns(
            new[]
        {
            new VisitedPage("https://x.com"), new VisitedPage("https://x.com/2")
        }.ToAsyncEnumerable());
        var service = builder.Build();

        _ = await service.SearchAsync("x").ToArrayAsync();

        builder.VisitedPageRepositoryMock.Verify(x => x.SearchAsync("x"), Times.Once);
    }
    
    [Fact]
    public async Task MarkVisitedPageAsync()
    {
        var builder = new VisitedPagesApplicationServiceMockBuilder();
        var service = builder.Build();

        await service.MarkVisitedPageAsync(new Uri("https://example.com/a"));

        builder.VisitedPageRepositoryMock.Verify(
            x => x.ExistsAsync(It.IsAny<Uri>()),
            Times.Never);
        builder.VisitedPageRepositoryMock.Verify(
            x => x.UpsertAsync(It.IsAny<Uri>()),
            Times.Once);
        builder.VisitedPageRepositoryMock.Verify(
            x => x.UpsertAsync(new Uri("https://example.com/a")),
            Times.Once);
    }
}
