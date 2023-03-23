using System.Diagnostics;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.System;

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
        var psi = new ProcessStartInfo(Path.Combine(_fixture.InstallFullPath, "scrap"))
        {
            Arguments = "sc",
            Environment =
            {
                ["Scrap_Scrap__GlobalConfigPath"] = "NotExisting",
                ["Scrap_Scrap__FileSystemType"] = "local"
            }
        };
        var (_, standardOutput, standardError, _) = psi.Run(outputWriter: new TestOutputHelperTextWriter(_output));

        standardError.Should().BeEmpty();
        standardOutput.Should().ContainMatch("*The tool is not properly configured; call 'scrap config'*");
    }
}
