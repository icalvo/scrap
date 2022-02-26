using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.ApplicationServices;

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
        var builder = new MockBuilder(
            _output,
            new PageMock("https://example.com/a"),
            new PageMock("https://example.com/b"));
        var jobDto = builder.BuildJobDto();
        var service = builder.BuildTraversalApplicationService(jobDto);

        var actual = await service.TraverseAsync(jobDto).ToArrayAsync();

        actual.Should().BeEquivalentTo("https://example.com/a", "https://example.com/b");
    }
}
