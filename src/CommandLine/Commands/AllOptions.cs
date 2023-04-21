using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("all", HelpText = "Executes all job definitions with a root URL")]
internal class AllOptions : OptionsBase, IScrapSettings
{
    [Option('f', "fullscan", Required = false, HelpText = "Navigate through already visited pages")]
    public bool FullScan { get; set; }

    [Option(
        'a',
        "downloadAlways",
        Required = false,
        HelpText = "Download resources even if they are already downloaded")]
    public bool DownloadAlways { get; set; }

    [Option('m', "disableMarkingVisited", Required = false, HelpText = "Disable mark as visited")]
    // [CommandOption("--dmv|--disableMarkingVisited")]
    public bool DisableMarkingVisited { get; set; }

    [Option(
        'w',
        "disableResourceWrite",
        Required = false,
        HelpText = "Download resources even if they are already downloaded")]
    //[CommandOption("--drw|--disableResourceWrite")]
    public bool DisableResourceWrites { get; set; }

    public override bool ConsoleLog => true;
}
