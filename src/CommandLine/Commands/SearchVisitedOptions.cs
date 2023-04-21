using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("searchvisited", aliases: new[] { "sv" }, HelpText = "Searches visited pages")]
internal sealed class SearchVisitedOptions : SearchOptions
{
    public override bool ConsoleLog => false;
}
