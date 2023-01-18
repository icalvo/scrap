using System.Diagnostics;
using System.Text;

namespace Scrap.Tests;

public sealed class FreshInstallSetupFixture : IDisposable
{
    public string InstallFullPath { get; }

    public FreshInstallSetupFixture()
    {
        const string version = "0.1.2-test1";
        const string mainVersion = "0.1.2";
        Environment.CurrentDirectory = Environment.CurrentDirectory.Split("src")[0] + "src";
        DirectoryEx.DeleteIfExists("./CommandLine/nupkg", recursive: true);
        DirectoryEx.DeleteIfExists("./testsite-result", recursive: true);

        RunAndCheck("dotnet",
            $"build ./CommandLine/CommandLine.csproj /p:Version=\"{version}\" /p:AssemblyVersion=\"{mainVersion}\" /p:FileVersion=\"{mainVersion}\" /p:InformationalVersion=\"{version}\"");
        RunAndCheck("dotnet", $"pack /p:PackageVersion=\"{version}\" --no-build");
        Run("dotnet", $"tool uninstall scrap --tool-path install");
        RunAndCheck("dotnet", $"tool install scrap --tool-path install --add-source ./CommandLine/nupkg/ --version 0.1.2-test1");
        InstallFullPath = Path.GetFullPath("./install");
    }

    public void Dispose()
    {
        Run("dotnet", "tool uninstall scrap --tool-path install");
        DirectoryEx.DeleteIfExists("./install", recursive: true);
        DirectoryEx.DeleteIfExists("./testsite-result", recursive: true);
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

        var (process, standardOutput, _) = psi.Run(timeout);
        
        if (checkExitCode && process.ExitCode != 0)
        {
            throw new Exception($"FAILED: {fileName} {arguments}\n" + string.Join("\n", standardOutput));
        }
    }

    private static void RunProcess(string fileName, string arguments, TimeSpan? timeout, bool checkExitCode,
        ProcessStartInfo psi)
    {
        Console.WriteLine($"Running {fileName} {arguments}...");
        var process = Process.Start(psi) ?? throw new Exception("Could not start process");

        var standardOutput = new StringBuilder();
        var cancellationTokenSource = new CancellationTokenSource();
        var finalTimeout = timeout ?? TimeSpan.FromSeconds(20);
        var timer = new Timer(
            _ => cancellationTokenSource.Cancel(),
            state: null,
            dueTime: finalTimeout,
            period: Timeout.InfiniteTimeSpan);
        process.OutputDataReceived += (_, args) =>
        {
            timer.Change(finalTimeout, Timeout.InfiniteTimeSpan);
            var line = args.Data;
            if (!string.IsNullOrWhiteSpace(line))
            {
                standardOutput.AppendLine(line);
            }
        };

        process.BeginOutputReadLine();

        while (!process.HasExited)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(500));
            if (cancellationTokenSource.Token.IsCancellationRequested)
            {
                throw new TimeoutException($"Could not run process in less than {finalTimeout}: {fileName} {arguments}");
            }
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

public static class ProcessRunner
{
    public static (Process process, List<string> standardOutput, List<string> standardError) Run(
        this ProcessStartInfo psi,
        TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(20);

        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;

        var fileName = psi.FileName;
        var arguments = psi.Arguments;

        Console.WriteLine($"Running {fileName} {arguments}...");
        var process = Process.Start(psi) ?? throw new Exception("Could not start process");

        var standardOutput = new List<string>();
        var standardError = new List<string>();
        var cancellationTokenSource = new CancellationTokenSource();
        var finalTimeout = timeout ?? TimeSpan.FromSeconds(20);
        var timer = new Timer(
            _ => cancellationTokenSource.Cancel(),
            state: null,
            dueTime: finalTimeout,
            period: Timeout.InfiniteTimeSpan);
        process.OutputDataReceived += (_, args) =>
        {
            timer.Change(finalTimeout, Timeout.InfiniteTimeSpan);
            var line = args.Data;
            if (!string.IsNullOrWhiteSpace(line))
            {
                standardOutput.Add(line);
            }
        };
        process.ErrorDataReceived += (_, args) =>
        {
            timer.Change(finalTimeout, Timeout.InfiniteTimeSpan);
            var line = args.Data;
            if (!string.IsNullOrWhiteSpace(line))
            {
                standardError.Add(line);
            }
        };
        process.BeginOutputReadLine();

        while (!process.HasExited)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(500));
            if (cancellationTokenSource.Token.IsCancellationRequested)
            {
                throw new TimeoutException($"Could not run process in less than {finalTimeout}: {fileName} {arguments}");
            }
        }

        return (process, standardOutput, standardError);
    }
} 
