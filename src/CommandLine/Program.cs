using System.Diagnostics;
using CLAP;
using Scrap.CommandLine;

TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
    Console.Error.WriteLine($"Unobserved exception: {eventArgs.Exception}");

var parser = new Parser<ScrapCommandLine>();
var scrapCommandLine = new ScrapCommandLine(parser, args);

try
{
    await parser.RunAsync(args, scrapCommandLine);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Uncaught exception: {ex.Demystify()}");
    throw;
}
