using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("deletevisited", aliases: new[] { "dv" }, HelpText = "Searches and removes visited pages")]
internal sealed class DeleteVisitedOptions : SearchOptions
{
    public override bool ConsoleLog => false;
}
