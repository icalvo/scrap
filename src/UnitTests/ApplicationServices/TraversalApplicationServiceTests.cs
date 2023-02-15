using FluentAssertions;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
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
        var jobDto = JobDtoBuilder.Build(ResourceType.DownloadLink);
        builder.JobFactoryMock.SetupFactory(new Job(jobDto));
        var service = builder.Build();

        var actual = await service.TraverseAsync(jobDto).ToArrayAsync();

        actual.Should().BeEquivalentTo("https://example.com/a", "https://example.com/b");
    }
}
