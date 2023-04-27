using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("deletevisited", aliases: new[] { "dv" }, HelpText = "Searches and removes visited pages")]
internal sealed class DeleteVisitedOptions : SearchOptions
{
    public DeleteVisitedOptions(string search, bool debug = false, bool verbose = false) : base(debug, verbose, search)
    {
    }

    public override bool ConsoleLog => false;


    [Usage]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Called by CommandLineParser")]
    public static IEnumerable<Example> Examples =>
        new[]
        {
            new Example(
                "Deletes all visited pages containing 'example.com'",
                new UnParserSettings { HideDefaultVerb = true },
                new DeleteVisitedOptions(".*example\\.com.*"))
        };
}
