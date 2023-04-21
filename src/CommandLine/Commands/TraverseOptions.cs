using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("traverse", aliases: new[] { "t" }, HelpText = "Lists all the pages reachable with the adjacency path")]
internal sealed class TraverseOptions : NameOrRootUrlOptions
{
    [Option('f', "fullscan", Required = false, HelpText = "Navigate through already visited pages")]
    public bool FullScan { get; set; }

    public override bool ConsoleLog => false;
}
