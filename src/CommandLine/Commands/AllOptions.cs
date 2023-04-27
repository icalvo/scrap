using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("all", HelpText = "Executes all job definitions with a root URL")]
internal class AllOptions : OptionsBase, IScrapOptions
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

    public bool FullScan { get; }

    public bool DownloadAlways { get; }

    public bool DisableMarkingVisited { get; }

    public bool DisableResourceWrites { get; }

    public override bool ConsoleLog => true;

    [Usage]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Called by CommandLineParser")]
    public static IEnumerable<Example> Examples =>
        new[] { new Example("Scrap all job defs. with root URL", new AllOptions()) };
}
