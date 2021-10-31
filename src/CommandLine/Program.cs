using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CLAP;
using Scrap.CommandLine;

TaskScheduler.UnobservedTaskException += (_, eventArgs) => Console.WriteLine(eventArgs.Exception);
Console.WriteLine("SCRAP");
Console.WriteLine("-----");
var parser = new Parser<ScrapperApplication>();
parser.Register.HelpHandler("help,h,?", (Action<string>) (Console.WriteLine));
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
