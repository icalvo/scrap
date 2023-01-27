﻿using System.Diagnostics;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.Integration;

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
            Environment =
            {
                ["Scrap_GlobalConfigurationFolder"] = "NotExisting"
            }
        };
        var (_, standardOutput, standardError, _) = psi.Run(outputWriter: new TestOutputHelperTextWriter(_output));

        standardError.Should().BeEmpty();
        standardOutput.Should().Contain("The tool is not properly configured; call 'scrap config'");
    }
}
