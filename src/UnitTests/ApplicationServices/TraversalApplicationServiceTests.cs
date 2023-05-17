using FluentAssertions;
using Moq;
using Scrap.Application.Traversal;
using Scrap.Domain;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit.ApplicationServices;

public class TraversalApplicationServiceTests
{
    private readonly ITestOutputHelper _output;

    public TraversalApplicationServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Traverse()
    {
        var builder = new TraversalApplicationServiceMockBuilder();
        builder.SetupTraversal(new PageMock("https://example.com/a"), new PageMock("https://example.com/b"));
        var job = JobBuilder.Build(ResourceType.DownloadLink);
        builder.JobServiceMock.SetupWithJob(job, "x");
        var service = builder.Build();

        var actual = await service.TraverseAsync(Mock.Of<ITraverseCommand>()).ToArrayAsync();

        actual.Should().BeEquivalentTo("https://example.com/a", "https://example.com/b");
    }
}
