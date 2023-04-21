using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("scrap", true, HelpText = "Executes a job definition")]
internal sealed class ScrapOptions : NameOrRootUrlOptions, IScrapSettings
{
    [Option('f', "fullscan", Required = false, HelpText = "Navigate through already visited pages")]
    public bool FullScan { get; set; }

    [Option(
        'a',
        "downloadAlways",
        Required = false,
        HelpText = "Download resources even if they are already downloaded")]
    public bool DownloadAlways { get; set; }

    [Option('m', new[] { "disableMarkingVisited", "dmv" }, Required = false, HelpText = "Disable mark as visited")]
    public bool DisableMarkingVisited { get; set; }

    [Option(
        'r',
        new[] { "disableResourceWrite", "drw" },
        Required = false,
        HelpText = "Download resources even if they are already downloaded")]
    public bool DisableResourceWrites { get; set; }

    public override bool ConsoleLog => true;
}
