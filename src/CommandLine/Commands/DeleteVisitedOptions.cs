using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("deletevisited", aliases: new[] { "dv" }, HelpText = "Searches and removes visited pages")]
internal sealed class DeleteVisitedOptions : SearchOptions
{
    public DeleteVisitedOptions(string search, bool debug = false, bool verbose = false) : base(debug, verbose, search)
    {
    }

    public override bool ConsoleLog => false;
}
