using System.Diagnostics;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests;

[Collection("Tool setup collection")]
public class IntegrationTests
{
    private readonly ToolSetupFixture _fixture;
    private readonly ITestOutputHelper _output;

    public IntegrationTests(ToolSetupFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }
    
    [Fact]
    public async Task CommandLine_Version()
    {
        var commandLineOutput = await GetCommandLineOutput("v").ToArrayAsync();
        commandLineOutput.Should().BeEquivalentTo("0.1.2-test1");
    }

    [Fact]
    public async Task CommandLine_Scrap_Simple()
    {
        var commandLineOutput = await GetCommandLineOutput("-name=testsite").ToListAsync();
        _output.WriteLine("-------------------------------------");
        commandLineOutput.ForEach(_output.WriteLine);
        _output.WriteLine("-------------------------------------");
        string? downloadedContent = null;
        if (File.Exists("./testsite-result/0.txt"))
        {
            downloadedContent = await File.ReadAllTextAsync("./testsite-result/0.txt");
        }

        downloadedContent.Should().Be("My text.");
    }

    private async IAsyncEnumerable<string> GetCommandLineOutput(string args)
    {
        var psi = new ProcessStartInfo(
            Path.Combine(_fixture.InstallFullPath, "scrap.exe"), args)
        {
            RedirectStandardOutput = true,
            Environment =
            {
                ["Scrap_GlobalConfigurationFolder"] = _fixture.InstallFullPath
            }
        };
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
