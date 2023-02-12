using System.Diagnostics;
using Scrap.Common;

namespace Scrap.Tests.Integration;

public sealed class ConfiguredFixture : FreshInstallSetupFixture
{
    private readonly string _dbFullPath;
    private readonly Process _serverProcess;

    public ConfiguredFixture()
    {
        Console.WriteLine($"Setting up {nameof(ConfiguredFixture)} (configured tool)");
        var jobDefsFullPath = Path.GetFullPath("./IntegrationTests/jobDefinitions.json");
        _dbFullPath = Path.GetFullPath("./scrap.db");
        RunAndCheck(
            $"{InstallFullPath}/scrap",
            $"config /key={ConfigKeys.Definitions} /value={jobDefsFullPath}",
            outputToConsole: true);
        RunAndCheck(
            $"{InstallFullPath}/scrap",
            $"config /key={ConfigKeys.Database} /value=\"Filename={_dbFullPath};Connection=shared\"");
        RunAndCheck("dotnet", $"tool install dotnet-serve --tool-path \"{InstallFullPath}\"");
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
        Run("dotnet", $"tool uninstall scrap --tool-path \"{InstallFullPath}\"");
        DirectoryEx.DeleteIfExists(InstallFullPath, true);
        DirectoryEx.DeleteIfExists("./testsite-result", true);
        File.Delete(_dbFullPath);
    }
}
