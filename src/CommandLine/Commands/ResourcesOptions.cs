using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb(
    "resources",
    aliases: new[] { "r" },
    HelpText = "Lists all the resources available in pages provided by console input")]
internal sealed class ResourcesOptions : NameOrRootUrlOptions
{
    [Option("pageUrls", Required = false, HelpText = "Page URLs [bold][[pipeline]][/]")]
    public string[]? PageUrls { get; }

    [Option(
        "onlyResourceLink",
        Required = false,
        HelpText = "Output only the resource link instead of the format expected by 'scrap download'")]
    public bool OnlyResourceLink { get; }

    public override bool ConsoleLog => false;
}
