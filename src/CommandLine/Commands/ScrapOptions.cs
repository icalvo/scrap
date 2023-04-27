using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by CommandLineParser")]
[Verb("scrap", true, HelpText = "Executes a job definition")]
internal sealed class ScrapOptions : NameOrRootUrlOptions, IScrapOptions
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

    public bool FullScan { get; }

    public bool DownloadAlways { get; }

    public bool DisableMarkingVisited { get; }

    public bool DisableResourceWrites { get; }

    public override bool ConsoleLog => true;

    [Usage]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Called by CommandLineParser")]
    public static IEnumerable<Example> Examples =>
        new[]
        {
            new Example(
                "Scrap job definition 'example'",
                new UnParserSettings { HideDefaultVerb = true },
                new ScrapOptions(nameOrRootUrlOption: "example")),
            new Example(
                "Find a job. def for the root URL 'https://example.com/page/41' and scraps it starting from that page",
                new UnParserSettings { HideDefaultVerb = true },
                new ScrapOptions(nameOrRootUrlOption: "https://example.com/page/41"))
        };
}
