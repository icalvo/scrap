using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("markvisited", aliases: new[] { "m", "mv" }, HelpText = "Adds a visited page")]
internal sealed class MarkVisitedOptions : OptionsBase
{
    public MarkVisitedOptions(bool debug, bool verbose, string[]? url) : base(debug, verbose)
    {
        Url = url;
    }

    [Option('u', "url", Required = false, HelpText = "URL [PIPELINE]")]
    public string[]? Url { get; }

    public override bool ConsoleLog => true;
}
