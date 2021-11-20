using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CLAP;
using Figgle;
using Scrap.CommandLine;

TaskScheduler.UnobservedTaskException += (_, eventArgs) => Console.WriteLine(eventArgs.Exception);

var currentColor = Console.ForegroundColor;
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine(FiggleFonts.Standard.Render("SCRAP"));
Console.WriteLine("Command line tool for generic web scrapping");
Console.ForegroundColor = currentColor;

var parser = new Parser<ScrapCommandLine>();
parser.Register.HelpHandler("help,h,?", s =>
{
    Console.WriteLine("SCRAP is a tool for generic web scrapping. To set it up, head to the project docs: https://ignaciocalvo.com/scrap");
    Console.WriteLine(s);
});
parser.Register.ParameterHandler("debug", (Action) (() => Debugger.Launch()));
parser.Register.ErrorHandler((Action<ExceptionContext>) (c =>
{
    Console.Error.WriteLine("Parsing error: {0}", c.Exception.Message);
    Console.Error.WriteLine("Arguments:");
    foreach (var (arg, idx) in args.Select((arg, idx) => (arg, idx)))
    {
        Console.WriteLine("Arg {0}: {1}", idx, arg);
    }
}));

await parser.RunTargetsAsync(args, (TargetResolver) null!);
