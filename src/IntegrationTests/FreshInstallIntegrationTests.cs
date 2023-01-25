using System.Diagnostics;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests;

[Collection(nameof(FreshInstallCollection))]
public class FreshInstallIntegrationTests
{
    private readonly FreshInstallSetupFixture _fixture;
    private readonly ITestOutputHelper _output;

    public FreshInstallIntegrationTests(FreshInstallSetupFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }
    
    [Fact]
    public void CommandLine_NotConfigured()
    {
        var psi = new ProcessStartInfo(Path.Combine(_fixture.InstallFullPath, "scrap.exe"))
        {
            Environment =
            {
                ["Scrap_GlobalConfigurationFolder"] = "C:\\NotExisting"
            }
        };
        var (_, standardOutput, standardError, _) = psi.Run(outputWriter: new TestOutputHelperTextWriter(_output));

        standardError.Should().BeEmpty();
        standardOutput.Should().BeEquivalentTo("The tool is not configured, please run 'scrap configure'.");
    }
}
