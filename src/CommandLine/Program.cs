using CLAP;
using Scrap.CommandLine;

TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
    Console.Error.WriteLine($"Unobserved exception: {eventArgs.Exception}");

var parser = new Parser<ScrapCommandLine>();

var scrapCommandLine = new ScrapCommandLine(parser, args);
await parser.RunAsync(args, scrapCommandLine);
