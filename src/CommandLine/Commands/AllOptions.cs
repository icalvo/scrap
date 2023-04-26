using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("all", HelpText = "Executes all job definitions with a root URL")]
internal class AllOptions : OptionsBase, IScrapSettings
{
    public AllOptions(
        bool fullScan = false,
        bool downloadAlways = false,
        bool disableMarkingVisited = false,
        bool disableResourceWrites = false,
        bool debug = false,
        bool verbose = false) : base(debug, verbose)
    {
        FullScan = fullScan;
        DownloadAlways = downloadAlways;
        DisableMarkingVisited = disableMarkingVisited;
        DisableResourceWrites = disableResourceWrites;
    }

    [Option('f', "fullscan", Required = false, HelpText = "Navigate through already visited pages")]
    public bool FullScan { get; }

    [Option(
        'a',
        "downloadAlways",
        Required = false,
        HelpText = "Download resources even if they are already downloaded")]
    public bool DownloadAlways { get; }

    [Option('m', new[] { "disableMarkingVisited", "dmv" }, Required = false, HelpText = "Disable mark as visited")]
    public bool DisableMarkingVisited { get; }

    [Option(
        'w',
        new[] { "disableResourceWrite", "drw" },
        Required = false,
        HelpText = "Download resources even if they are already downloaded")]
    public bool DisableResourceWrites { get; }

    public override bool ConsoleLog => true;

    [Usage]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Called by CommandLineParser")]
    public static IEnumerable<Example> Examples =>
        new[] { new Example("Scrap all job defs. with root URL", new AllOptions()) };
}
