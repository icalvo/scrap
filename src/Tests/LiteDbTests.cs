using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Scrap.Jobs.Graphs;
using Xunit;

namespace Scrap.Tests
{
    public class GraphSearchTests
    {
        [Fact]
        public void DepthFirstSearch_Test1()
        {
            var adj = new Dictionary<string, string[]>
            {
                { "a", new[] { "b", "c" } },
                { "b", new[] { "a" } },
                { "c", Array.Empty<string>() }
            };
            var dfs = new DepthFirstGraphSearch();
            var result = dfs.SearchAsync("a", Task.FromResult, s => adj[s].ToAsyncEnumerable()).ToEnumerable();

            result.Should().BeEquivalentTo("a", "b", "c");
        }
    }
}