using System.Diagnostics;

namespace Scrap.Tests.Integration;

public class FreshInstallSetupFixture : IDisposable
{
    public string InstallFullPath { get; }

    public FreshInstallSetupFixture()
    {
        const string version = "0.1.2-test1";
        const string mainVersion = "0.1.2";

        Console.WriteLine($"Setting up {nameof(FreshInstallSetupFixture)} (freshly installed tool, not configured)");
        
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
    }

    public virtual void Dispose()
    {
        Run("dotnet", $"tool uninstall scrap --tool-path \"{InstallFullPath}\"");
        DirectoryEx.DeleteIfExists(InstallFullPath, recursive: true);
        DirectoryEx.DeleteIfExists("./testsite-result", recursive: true);
    }

    protected void RunAndCheck(string fileName, string arguments, TimeSpan? timeout = null, bool outputToConsole = false)
    {
        Run(fileName, arguments, timeout, checkExitCode: true, outputToConsole);
    }

    protected void Run(string fileName, string arguments = "", TimeSpan? timeout = null, bool outputToConsole = false)
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
        var (process, standardOutput, errorOutput, output) = psi.Run(timeout, outputWriter: outputWriter);
        
        if (checkExitCode && process.ExitCode != 0)
        {
            throw new Exception($"FAILED: {fileName} {arguments}\n{string.Join("\n", output)}");
        }
    }
}
