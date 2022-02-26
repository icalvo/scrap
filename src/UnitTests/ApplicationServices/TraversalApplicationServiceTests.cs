using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests;

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
            TestTools.PageMock(
                "https://example.com/a",
                MockBuilder.LinkXPath, new[] { "https://example.com/b" },
                MockBuilder.ResourceXPath, new[] { "https://example.com/1.txt", "https://example.com/2.txt" },
                MockBuilder.ResourceXPath, new[] { "qwer", "asdf" }),
            TestTools.PageMock(
                "https://example.com/b",
                MockBuilder.LinkXPath, new[] { "https://example.com/a" },
                MockBuilder.ResourceXPath, new[] { "https://example.com/3.txt", "http://example.com/4.txt" },
                MockBuilder.ResourceXPath, new[] { "zxcv", "yuio" }));
        var jobDto = builder.BuildJobDto();
        var service = builder.BuildTraversalApplicationService(jobDto);

        var actual = await service.TraverseAsync(jobDto).ToArrayAsync();

        actual.Should().BeEquivalentTo("https://example.com/a", "https://example.com/b");
    }
}
