using System.Diagnostics;
using Xunit.Abstractions;

namespace Scrap.Tests.System;

public class FreshInstallSetupFixture : IDisposable
{
    protected readonly IMessageSink Output;

    public FreshInstallSetupFixture(IMessageSink output)
    {
        Output = output;
        const string version = "0.1.2-test1";
        const string mainVersion = "0.1.2";

        Console.WriteLine($"Setting up {nameof(FreshInstallSetupFixture)} (freshly installed tool, not configured)");

        InstallFullPath = Path.GetFullPath("./install");

        Environment.CurrentDirectory = $"{Environment.CurrentDirectory.Split("src")[0]}src";
        DirectoryEx.DeleteIfExists("./CommandLine/nupkg", true);
        DirectoryEx.DeleteIfExists("./testsite-result", true);
        Directory.CreateDirectory("./testsite-result");

        DirectoryEx.DeleteIfExists(InstallFullPath, true);
        Directory.CreateDirectory(InstallFullPath);

        RunAndCheck(
            "dotnet",
            $"build ./CommandLine/CommandLine.csproj /p:Version=\"{version}\" /p:AssemblyVersion=\"{mainVersion}\" /p:FileVersion=\"{mainVersion}\" /p:InformationalVersion=\"{version}\"");
        RunAndCheck("dotnet", $"pack /p:PackageVersion=\"{version}\" --no-build");
        RunAndCheck(
            "dotnet",
            $"tool install scrap --tool-path \"{InstallFullPath}\" --add-source ./CommandLine/nupkg/ --version {version}");
    }

    public string InstallFullPath { get; }

    protected void RunAndCheck(
        string fileName,
        string arguments,
        TimeSpan? timeout = null,
        bool outputToConsole = false) =>
        Run(fileName, arguments, timeout, true, outputToConsole);

    protected void Run(
        string fileName,
        string arguments = "",
        TimeSpan? timeout = null,
        bool outputToConsole = false) =>
        Run(fileName, arguments, timeout, false, outputToConsole);

    private void Run(string fileName, string arguments, TimeSpan? timeout, bool checkExitCode, bool outputToConsole)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            Environment =
            {
                ["Scrap_Scrap__GlobalConfigPath"] = Path.Combine(InstallFullPath, "scrap-user.json"),
                ["Scrap_Scrap__FileSystemType"] = "local"
            }
        };

        // var outputWriter = outputToConsole? Console.Out : null;

        var outputWriter = new MessageSinkTextWriter(Output);
        var (process, standardOutput, errorOutput, output) = psi.Run(timeout, outputWriter);

        if (checkExitCode && process.ExitCode != 0)
        {
            throw new Exception($"FAILED: {fileName} {arguments}\n{string.Join("\n", output)}");
        }
    }

    public virtual void Dispose()
    {
        Run("dotnet", $"tool uninstall scrap --tool-path \"{InstallFullPath}\"");
        DirectoryEx.DeleteIfExists(InstallFullPath, true);
        DirectoryEx.DeleteIfExists("./testsite-result", true);
    }
}
