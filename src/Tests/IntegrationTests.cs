using System.Diagnostics;
using FluentAssertions;
using Xunit;

namespace Scrap.Tests;

public class IntegrationTests
{
    [Fact]
    public async Task CommandLine_Version()
    {
        var commandLineOutput = await GetCommandLineOutput("v").ToArrayAsync();
        commandLineOutput.Should().BeEquivalentTo("0.1.2-test1");
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
