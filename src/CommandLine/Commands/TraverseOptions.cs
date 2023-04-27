using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("traverse", aliases: new[] { "t" }, HelpText = "Lists all the pages reachable with the adjacency path")]
internal sealed class TraverseOptions : NameOrRootUrlOptions, IFullScanOption
{
    public TraverseOptions(
        string? nameOrRootUrlOption = null,
        string? nameOption = null,
        string? rootUrlOption = null,
        bool fullScan = false,
        bool debug = false,
        bool verbose = false) : base(debug, verbose, nameOrRootUrlOption, nameOption, rootUrlOption)
    {
        FullScan = fullScan;
    }

    public bool FullScan { get; }

    public override bool ConsoleLog => false;


    [Usage]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Called by CommandLineParser")]
    public static IEnumerable<Example> Examples =>
        new[]
        {
            new Example(
                "Traverse job def. 'example' and outputs all traversed pages",
                new TraverseOptions("example")),
            new Example(
                "Traverse job def. 'example', without taking visited pages into account, and outputs all traversed pages",
                new TraverseOptions("example", fullScan: true))
        };    
}
