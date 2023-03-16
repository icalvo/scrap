using System.Collections.Concurrent;
using System.Diagnostics;

namespace Scrap.Tests.System;

public static class ProcessRunner
{
    public static (Process process, List<string> standardOutput, List<string> standardError, string[] output) Run(
        this ProcessStartInfo psi,
        TimeSpan? timeout = null,
        TextWriter? outputWriter = null)
    {
        timeout ??= TimeSpan.FromSeconds(20);

        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;

        var fileName = psi.FileName;
        var arguments = psi.Arguments;

        (outputWriter ?? Console.Out).WriteLine($"Running {fileName} {arguments}...");
        var process = Process.Start(psi) ?? throw new Exception("Could not start process");

        var standardOutput = new List<string>();
        var standardError = new List<string>();
        var cancellationTokenSource = new CancellationTokenSource();
        var timer = new Timer(_ => cancellationTokenSource.Cancel(), null, timeout.Value, Timeout.InfiniteTimeSpan);
        var output = new ConcurrentQueue<string>();
        process.OutputDataReceived += (_, args) =>
        {
            timer.Change(timeout.Value, Timeout.InfiniteTimeSpan);
            var line = args.Data;
            outputWriter?.WriteLine(line);

            if (!string.IsNullOrWhiteSpace(line))
            {
                standardOutput.Add(line);
                output.Enqueue(line);
            }
        };
        process.ErrorDataReceived += (_, args) =>
        {
            timer.Change(timeout.Value, Timeout.InfiniteTimeSpan);
            var line = args.Data;
            outputWriter?.WriteLine(line);

            if (!string.IsNullOrWhiteSpace(line))
            {
                standardError.Add(line);
                output.Enqueue(line);
            }
        };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        while (!process.HasExited)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(500));
            if (cancellationTokenSource.Token.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"Could not run process in less than {timeout.Value}: {fileName} {arguments}");
            }
        }

        return (process, standardOutput, standardError, output.ToArray());
    }
}
