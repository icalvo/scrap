﻿using System.Diagnostics;
using Scrap.Domain;
using Xunit.Abstractions;

namespace Scrap.Tests.System;

public sealed class ConfiguredFixture : FreshInstallSetupFixture
{
    private readonly string _dbFullPath;
    private readonly Process _serverProcess;

    public ConfiguredFixture(IMessageSink output) : base(output)
    {
        Console.WriteLine($"Setting up {nameof(ConfiguredFixture)} (configured tool)");
        var sitesFullPath = Path.GetFullPath("./SystemTests/sites.json");
        _dbFullPath = Path.GetFullPath("./scrap.db");
        RunAndCheck(
            $"{InstallFullPath}/scrap",
            $"config {ConfigKeys.Sites} {sitesFullPath}",
            outputToConsole: true);
        RunAndCheck(
            $"{InstallFullPath}/scrap",
            $"config {ConfigKeys.Database} \"Filename={_dbFullPath};Connection=shared\"");

        foreach (var process in Process.GetProcessesByName("dotnet-serve"))
        {
            process.Kill();
        }

        RunAndCheck("dotnet", $"tool install dotnet-serve --tool-path \"{InstallFullPath}\"", TimeSpan.FromSeconds(40));
        var wwwPath = Path.GetFullPath("./IntegrationTests/www/");

        var psi = new ProcessStartInfo
        {
            FileName = $"{InstallFullPath}/dotnet-serve",
            Arguments = $"--directory {wwwPath} --port 8080",
            WorkingDirectory = "."
        };
        _serverProcess = Process.Start(psi) ?? throw new Exception("Could not start server");
    }

    public override void Dispose()
    {
        _serverProcess.Kill();
        Run("dotnet", $"tool uninstall dotnet-serve --tool-path \"{InstallFullPath}\"");
        File.Delete(_dbFullPath);
        base.Dispose();
    }
}
