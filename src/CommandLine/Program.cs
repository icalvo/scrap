using System;
using System.Threading.Tasks;
using CLAP;
using Scrap.CommandLine;

TaskScheduler.UnobservedTaskException += (sender, eventArgs) => Console.WriteLine(eventArgs.Exception); 
Parser.RunConsole<ScrapperApplication>(args);
