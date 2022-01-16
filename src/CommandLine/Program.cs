using System.Diagnostics;
using CLAP;
using Scrap.CommandLine;

TaskScheduler.UnobservedTaskException += (_, eventArgs) => Console.Error.WriteLine("Unobserved: " + eventArgs.Exception);

var parser = new Parser<ScrapCommandLine>();
parser.Register.HelpHandler("help,h,?", s =>
{
    Console.WriteLine("SCRAP is a tool for generic web scrapping. To set it up, head to the project docs: https://github.com/icalvo/scrap");
    Console.WriteLine(s);
});
parser.Register.ErrorHandler((Action<ExceptionContext>) (c =>
{
    Console.Error.WriteLine("Parsing error: {0}", c.Exception.Demystify());
    Console.Error.WriteLine("Arguments:");
    foreach (var (arg, idx) in args.Select((arg, idx) => (arg, idx)))
    {
        Console.WriteLine("Arg {0}: {1}", idx, arg);
    }
}));

try
{
    await parser.RunAsync(args, new ScrapCommandLine());
}
catch (Exception ex)
{
    Console.Error.WriteLine("Uncatched: " + ex.Demystify());
    throw;
}
