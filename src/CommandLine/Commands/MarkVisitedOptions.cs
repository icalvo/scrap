using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("markvisited", aliases: new[] { "m", "mv" }, HelpText = "Adds a visited page")]
internal sealed class MarkVisitedOptions : OptionsBase
{
    public MarkVisitedOptions(string[] urls, bool debug = false, bool verbose = false) : base(debug, verbose)
    {
        Urls = urls;
    }

    [Option('u', "urls", Required = false, HelpText = "URL [PIPELINE]")]
    public string[] Urls { get; }

    public override bool ConsoleLog => true;

    [Usage]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Called by CommandLineParser")]
    public static IEnumerable<Example> Examples =>
        new[]
        {
            new Example(
                "Marks as visited the page 'https://example.com/page/342'",
                new MarkVisitedOptions(new[] { "https://example.com/page/342" }))
        };    
}
