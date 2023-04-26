using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by CommandLineParser")]
[Verb("scrap", true, HelpText = "Executes a job definition")]
internal sealed class ScrapOptions : NameOrRootUrlOptions, IScrapSettings
{
    public ScrapOptions(
        bool debug = false,
        bool verbose = false,
        string? nameOrRootUrlOption = null,
        string? nameOption = null,
        string? rootUrlOption = null,
        bool fullScan = false,
        bool downloadAlways = false,
        bool disableMarkingVisited = false,
        bool disableResourceWrites = false) : base(debug, verbose, nameOrRootUrlOption, nameOption, rootUrlOption)
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
        'r',
        new[] { "disableResourceWrite", "drw" },
        Required = false,
        HelpText = "Download resources even if they are already downloaded")]
    public bool DisableResourceWrites { get; }

    public override bool ConsoleLog => true;

    [Usage]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Called by CommandLineParser")]
    public static IEnumerable<Example> Examples =>
        new[]
        {
            new Example(
                "Scrap a job definition",
                new UnParserSettings { HideDefaultVerb = true },
                new ScrapOptions(nameOrRootUrlOption: "myjobdefinition")),
            new Example(
                "Find a job. def from a root URL and execute it",
                new UnParserSettings { HideDefaultVerb = true },
                new ScrapOptions(nameOrRootUrlOption: "https://example.com/page/41"))
        };
}
