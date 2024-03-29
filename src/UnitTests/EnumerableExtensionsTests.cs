﻿using FluentAssertions;
using Scrap.Common;
using Xunit;

namespace Scrap.Tests.Unit;

public class EnumerableExtensionsTests
{
    [Fact]
    public void ForEach()
    {
        IEnumerable<int> a = new[] { 1, 5, 2 };
        var result = "";
        a.ForEach(x => result += x + ",", () => { });

        result.Should().Be("1,5,2,");
    }

    [Fact]
    public void ForEach_EmptySource()
    {
        var a = Enumerable.Empty<int>();
        var result = "";
        a.ForEach(x => result += x + ",", () => result = "empty");

        result.Should().Be("empty");
    }

    [Fact]
    public async Task ExecuteAsync()
    {
        var result = "";
        var a = new[] { 1, 5, 2 }.ToAsyncEnumerable();
        var b = a.Do(x => result += x + ",");
        result.Should().Be("");
        await b.ExecuteAsync();
        result.Should().Be("1,5,2,");
    }

    [Fact]
    public async Task Do()
    {
        var a = new[] { 1, 5, 2 }.ToAsyncEnumerable();


        var result = "";
        var b = a.Do(x => result += x + ",");
        result.Should().Be("");
        var arrayAsync = await b.ToArrayAsync();
        result.Should().Be("1,5,2,");
        arrayAsync.Should().Equal(1, 5, 2);
    }

    [Fact]
    public async Task DoAwait1()
    {
        var a = new[] { 1, 5, 2 }.ToAsyncEnumerable();


        var result = "";
        var b = a.DoAwait(
            x =>
            {
                result += x + ",";
                return Task.CompletedTask;
            });
        result.Should().Be("");
        var arrayAsync = await b.ToArrayAsync();
        result.Should().Be("1,5,2,");
        arrayAsync.Should().Equal(1, 5, 2);
    }

    [Fact]
    public async Task DoAwait2()
    {
        var a = new[] { 1, 5, 2 }.ToAsyncEnumerable();


        var result = "";
        var b = a.DoAwait(
            (x, i) =>
            {
                result += $"{i}-{x},";
                return Task.CompletedTask;
            });
        result.Should().Be("");
        var arrayAsync = await b.ToArrayAsync();
        result.Should().Be("0-1,1-5,2-2,");
        arrayAsync.Should().Equal(1, 5, 2);
    }

    [Fact]
    public void RemoveNulls()
    {
        var a = new int?[] { 2, null, 0, null, 3, 4 };

        a.RemoveNulls().Should().Equal(2, 0, 3, 4);
    }
}
