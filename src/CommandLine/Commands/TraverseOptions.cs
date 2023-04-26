using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("traverse", aliases: new[] { "t" }, HelpText = "Lists all the pages reachable with the adjacency path")]
internal sealed class TraverseOptions : NameOrRootUrlOptions
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

    [Option('f', "fullscan", Required = false, HelpText = "Navigate through already visited pages")]
    public bool FullScan { get; }

    public override bool ConsoleLog => false;
}
