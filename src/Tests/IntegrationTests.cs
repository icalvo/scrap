using System.Diagnostics;
using FluentAssertions;
using LamarCodeGeneration.Util;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests;

public class IntegrationTests
{
    private readonly ITestOutputHelper _output;

    public IntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public async Task CommandLine_Version()
    {
        var commandLineOutput = await GetCommandLineOutput("v").ToArrayAsync();
        commandLineOutput.Should().BeEquivalentTo("0.1.2-test1");
    }

    [Fact]
    public async Task SimpleScrap_Version()
    {
        var commandLineOutput = await GetCommandLineOutput("-name=testsite").ToArrayAsync();
        _output.WriteLine("-------------------------------------");
        commandLineOutput.ForEach(_output.WriteLine);
        _output.WriteLine("-------------------------------------");
        var downloadedContent = await File.ReadAllTextAsync("./tricky/0.txt");
        downloadedContent.Should().Be("My text.");
    }

    private static async IAsyncEnumerable<string> GetCommandLineOutput(string args)
    {
        var psi = new ProcessStartInfo("scrap", args) { RedirectStandardOutput = true, };
        var p = Process.Start(psi);
        Debug.Assert(p != null);
        while (true)
        {
            var line = await p.StandardOutput.ReadLineAsync();
            if (line == null) yield break;
            yield return line;
        }
    }    
}
