using System.Diagnostics;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.System;

[Collection(nameof(ConfiguredCollection))]
public class ConfiguredIntegrationTests
{
    private readonly ConfiguredFixture _fixture;
    private readonly ITestOutputHelper _output;

    public ConfiguredIntegrationTests(ConfiguredFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public void CommandLine_Version()
    {
        var commandLineOutput = GetCommandLineOutput("version").ToArray();
        commandLineOutput.Should().BeEquivalentTo("scrap 0.1.2-test1");
    }

    [Fact]
    public async Task CommandLine_Scrap_Simple()
    {
        _ = GetCommandLineOutput("testsite -v -d").ToList();
        string? downloadedContent = null;
        if (File.Exists("./testsite-result/0.txt"))
        {
            downloadedContent = await File.ReadAllTextAsync("./testsite-result/0.txt");
        }
        else
        {
            Assert.Fail("The expected downloaded result was not found!");
        }

        downloadedContent.Should().Be("My text.");
    }

    private IEnumerable<string> GetCommandLineOutput(string args, string? configFolderPath = null)
    {
        configFolderPath ??= _fixture.InstallFullPath;
        var psi = new ProcessStartInfo(Path.Combine(_fixture.InstallFullPath, "scrap"), args)
        {
            RedirectStandardOutput = true,
            Environment =
            {
                ["Scrap_Scrap__GlobalConfigPath"] = Path.Combine(configFolderPath, "scrap-user.json"),
                ["Scrap_Scrap__FileSystemType"] = "local"
            }
        };

        var (_, _, _, output) = psi.Run(
            outputWriter: new TestOutputHelperTextWriter(_output),
            timeout: TimeSpan.FromDays(2));

        return output;
    }
}
