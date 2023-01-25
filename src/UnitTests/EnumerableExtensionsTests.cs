using FluentAssertions;
using Scrap.Domain;
using Xunit;

namespace Scrap.Tests.Unit;

public class EnumerableExtensionsTests
{
    [Fact]
    public void ForEach()
    {
        IEnumerable<int> a = new[] { 1, 5, 2 };
        string result = "";
        a.ForEach(x => result += x + ",", () => {});

        result.Should().Be("1,5,2,");
    }

    [Fact]
    public void ForEach_EmptySource()
    {
        IEnumerable<int> a = Enumerable.Empty<int>();
        string result = "";
        a.ForEach(x => result += x + ",", () => result = "empty");

        result.Should().Be("empty");
    }
    

    [Fact]
    public async Task Do()
    {
        var a = new[] { 1, 5, 2 }.ToAsyncEnumerable();
        
        
        string result = "";
        var b = a.Do(x => result += x + ",");
        result.Should().Be("");
        int[] arrayAsync = (await b.ToArrayAsync());
        result.Should().Be("1,5,2,");
        arrayAsync.Should().Equal(1, 5, 2);
    }
}
