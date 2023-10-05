using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;
using Scrap.Application.Scrap.All;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("all", HelpText = "Executes all sites with a root URL")]
internal class ScrapAllOptions : OptionsBase, IScrapOptions, IScrapAllCommand
{
    public ScrapAllOptions(
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

    public bool FullScan { get; }

    public bool DownloadAlways { get; }

    public bool DisableMarkingVisited { get; }

    public bool DisableResourceWrites { get; }

    public override bool ConsoleLog => true;

    [Usage]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Called by CommandLineParser")]
    public static IEnumerable<Example> Examples =>
        new[] { new Example("Scrap all sites with root URL", new ScrapAllOptions()) };
}
