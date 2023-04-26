using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Scrap.Common.Graphs;
using Xunit;

namespace Scrap.Tests.Unit;

public class GraphSearchTests
{
    [Fact]
    public void DepthFirstSearch_Test1()
    {
        var adj = new Dictionary<string, string[]>
        {
            { "a", new[] { "b", "c" } }, { "b", new[] { "a" } }, { "c", Array.Empty<string>() }
        };
        var dfs = new DepthFirstGraphSearch(Mock.Of<ILogger<DepthFirstGraphSearch>>());
        var result = dfs.SearchAsync("a", Task.FromResult, s => adj[s].ToAsyncEnumerable()).ToEnumerable();

        result.Should().BeEquivalentTo("a", "b", "c");
    }
}
