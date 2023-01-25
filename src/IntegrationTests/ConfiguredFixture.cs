using System.Diagnostics;

namespace Scrap.Tests;

public sealed class ConfiguredFixture : IDisposable
{
    private readonly Process _serverProcess;
    private readonly string _dbFullPath;
    public string InstallFullPath { get; }

    public ConfiguredFixture()
    {
        const string version = "0.1.2-test1";
        const string mainVersion = "0.1.2";
        
        
        Console.WriteLine($"Setting up {nameof(ConfiguredFixture)} (configured tool with http server on test files)");
        InstallFullPath = Path.GetFullPath("./install");

        Environment.CurrentDirectory = $"{Environment.CurrentDirectory.Split("src")[0]}src";
        DirectoryEx.DeleteIfExists("./CommandLine/nupkg", recursive: true);
        DirectoryEx.DeleteIfExists("./testsite-result", recursive: true);
        foreach (var process in Process.GetProcessesByName("dotnet-serve"))
        {
            process.Kill();
        }
        DirectoryEx.DeleteIfExists(InstallFullPath, recursive: true);
        Directory.CreateDirectory(InstallFullPath);

        RunAndCheck("dotnet",
            $"build ./CommandLine/CommandLine.csproj /p:Version=\"{version}\" /p:AssemblyVersion=\"{mainVersion}\" /p:FileVersion=\"{mainVersion}\" /p:InformationalVersion=\"{version}\"");
        RunAndCheck("dotnet", $"pack /p:PackageVersion=\"{version}\" --no-build");
        RunAndCheck("dotnet", $"tool install scrap --tool-path \"{InstallFullPath}\" --add-source ./CommandLine/nupkg/ --version {version}");
        var jobDefsFullPath = Path.GetFullPath("./IntegrationTests/jobDefinitions.json");
        _dbFullPath = Path.GetFullPath("./scrap.db");
        RunAndCheck($"{InstallFullPath}/scrap.exe", $"config /key=Scrap:Definitions /value={jobDefsFullPath}", outputToConsole: true);
        RunAndCheck($"{InstallFullPath}/scrap.exe", $"config /key=Scrap:Database /value=\"Filename={_dbFullPath};Connection=shared\"");
        RunAndCheck("dotnet", $"tool install dotnet-serve --tool-path \"{InstallFullPath}\"");
        var wwwPath = Path.GetFullPath("./IntegrationTests/www/");
        
        
        var psi = new ProcessStartInfo
        {
            FileName = $"{InstallFullPath}/dotnet-serve.exe",
            Arguments = $"--directory {wwwPath} --port 8080",
            WorkingDirectory = "."
        };
        _serverProcess = Process.Start(psi) ?? throw new Exception("Could not start server");
    }

    public void Dispose()
    {
        _serverProcess.Kill();
        Run("dotnet", $"tool uninstall dotnet-serve --tool-path \"{InstallFullPath}\"");
        Run("dotnet", $"tool uninstall scrap --tool-path \"{InstallFullPath}\"");
        DirectoryEx.DeleteIfExists(InstallFullPath, recursive: true);
        DirectoryEx.DeleteIfExists("./testsite-result", recursive: true);
        File.Delete(_dbFullPath);
    }

    private void RunAndCheck(string fileName, string arguments, TimeSpan? timeout = null, bool outputToConsole = false)
    {
        Run(fileName, arguments, timeout, checkExitCode: true, outputToConsole);
    }

    private void Run(string fileName, string arguments, TimeSpan? timeout = null, bool outputToConsole = false)
    {
        Run(fileName, arguments, timeout, checkExitCode: false, outputToConsole);
    }

    private void Run(string fileName, string arguments, TimeSpan? timeout, bool checkExitCode, bool outputToConsole)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            Environment =
            {
                ["Scrap_GlobalConfigurationFolder"] = InstallFullPath
            }
        };

        // var outputWriter = outputToConsole? Console.Out : null;
        var outputWriter = Console.Out;
        var (process, standardOutput, _, _) = psi.Run(timeout, outputWriter);

        if (checkExitCode && process.ExitCode != 0)
        {
            throw new Exception($"FAILED: {fileName} {arguments}\n{string.Join("\n", standardOutput)}\n");
        }
    }
}

