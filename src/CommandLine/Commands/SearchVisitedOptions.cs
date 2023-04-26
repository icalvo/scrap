using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("searchvisited", aliases: new[] { "sv" }, HelpText = "Searches visited pages")]
internal sealed class SearchVisitedOptions : SearchOptions
{
    public SearchVisitedOptions(string search, bool debug = false, bool verbose = false) : base(debug, verbose, search)
    {
    }

    public override bool ConsoleLog => false;
}
