using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using CLAP;
using Scrap.CommandLine;

TaskScheduler.UnobservedTaskException += (sender, eventArgs) => Console.WriteLine(eventArgs.Exception);

var parser = new Parser<ScrapperApplication>();
parser.Register.HelpHandler("help,h,?", (Action<string>) (Console.WriteLine));
parser.Register.ParameterHandler("debug", (Action) (() => Debugger.Launch()));
parser.Register.ErrorHandler((Action<ExceptionContext>) (c => Console.Error.WriteLine(c.Exception.Message)));
await parser.RunTargetsAsync(args, (TargetResolver) null!);
