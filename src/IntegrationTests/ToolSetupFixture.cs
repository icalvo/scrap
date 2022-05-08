using System.Diagnostics;

namespace Scrap.Tests;

public sealed class ToolSetupFixture : IDisposable
{
    private readonly Process _serverProcess;
    private readonly string _dbFullPath;
    public string InstallFullPath { get; }

    public ToolSetupFixture()
    {
        const string version = "0.1.2-test1";
        const string mainVersion = "0.1.2";
        Environment.CurrentDirectory = Environment.CurrentDirectory.Split("src")[0] + "src";
        DirectoryEx.DeleteIfExists("./CommandLine/nupkg", recursive: true);
        DirectoryEx.DeleteIfExists("./testsite-result", recursive: true);

        RunAndCheck("dotnet",
            $"build ./CommandLine/CommandLine.csproj /p:Version=\"{version}\" /p:AssemblyVersion=\"{mainVersion}\" /p:FileVersion=\"{mainVersion}\" /p:InformationalVersion=\"{version}\"",
            TimeSpan.FromSeconds(60));
        RunAndCheck("dotnet", $"pack /p:PackageVersion=\"{version}\" --no-build");
        Run("dotnet", $"tool uninstall scrap --tool-path install");
        RunAndCheck("dotnet", $"tool install scrap --tool-path install --add-source ./CommandLine/nupkg/ --version 0.1.2-test1");
        InstallFullPath = Path.GetFullPath("./install");
        var jobDefsFullPath = Path.GetFullPath("./IntegrationTests/jobDefinitions.json");
        _dbFullPath = Path.GetFullPath("./scrap.db");
        RunAndCheck("./install/scrap.exe", $"config /key=Scrap:Definitions /value={jobDefsFullPath}");
        RunAndCheck("./install/scrap.exe", $"config /key=Scrap:Database /value=\"Filename={_dbFullPath};Connection=shared\"");
        Run("dotnet", "tool uninstall dotnet-serve --tool-path install");
        RunAndCheck("dotnet", "tool install dotnet-serve --tool-path install");
        var wwwPath = Path.GetFullPath("./IntegrationTests/www/");
        var psi = new ProcessStartInfo
        {
            FileName = "./install/dotnet-serve.exe",
            Arguments = $"--directory {wwwPath} --port 8080",
            WorkingDirectory = "."
        };
        _serverProcess = Process.Start(psi) ?? throw new Exception("Could not start server");
    }

    public void Dispose()
    {
        _serverProcess.Kill();
        Run("dotnet", "tool uninstall dotnet-serve --tool-path install");
        Run("dotnet", "tool uninstall scrap --tool-path install");
        DirectoryEx.DeleteIfExists("./install", recursive: true);
        DirectoryEx.DeleteIfExists("./testsite-result", recursive: true);
        File.Delete(_dbFullPath);
    }

    private void RunAndCheck(string fileName, string arguments, TimeSpan? timeout = null)
    {
        Run(fileName, arguments, timeout, checkExitCode: true);
    }

    private void Run(string fileName, string arguments, TimeSpan? timeout = null)
    {
        Run(fileName, arguments, timeout, checkExitCode: false);
    }

    private void Run(string fileName, string arguments, TimeSpan? timeout, bool checkExitCode)
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
        var process = Process.Start(psi) ?? throw new Exception("Could not start process");
        var finalTimeout = timeout ?? TimeSpan.FromSeconds(20);
        if (!process.WaitForExit((int)finalTimeout.TotalMilliseconds))
        {
            throw new Exception($"Could not run process in less than {finalTimeout}: {fileName} {arguments}");
        }

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        var output = GetCommandLineOutput(process, cts.Token).ToArray();

        if (checkExitCode && process.ExitCode != 0)
        {
            throw new Exception($"FAILED: {fileName} {arguments}\n" + string.Join("\n", output));
        }
    }

    private static IEnumerable<string> GetCommandLineOutput(Process p, CancellationToken token)
    {
        while (true)
        {
            var line = p.StandardOutput.ReadLine();
            if (line == null) yield break;
            yield return line;
            if (token.IsCancellationRequested)
            {
                yield break;
            }
        }
    }
}
