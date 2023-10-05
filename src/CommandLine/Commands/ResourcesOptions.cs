using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb(
    "resources",
    aliases: new[] { "r" },
    HelpText = "Lists all the resources available in pages provided by console input")]
internal sealed class ResourcesOptions : NameOrRootUrlOptions
{
    public ResourcesOptions(
        string[] pageUrls,
        string? nameOrRootUrlOption = null,
        string? nameOption = null,
        string? rootUrlOption = null,
        bool onlyResourceLink = false,
        bool debug = false,
        bool verbose = false) : base(debug, verbose, nameOrRootUrlOption, nameOption, rootUrlOption)
    {
        PageUrls = pageUrls;
        OnlyResourceLink = onlyResourceLink;
    }

    [Option('u', "urls", Required = false, HelpText = "Page URLs [PIPELINE]")]
    public string[] PageUrls { get; }

    [Option(
        "onlyResourceLink",
        Required = false,
        HelpText = "Output only the resource link instead of the format expected by 'scrap download'")]
    public bool OnlyResourceLink { get; }

    public override bool ConsoleLog => false;


    [Usage]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Called by CommandLineParser")]
    public static IEnumerable<Example> Examples =>
        new[]
        {
            new Example(
                "Lists resource from page 'https://example.com/page/342', site 'example'",
                new ResourcesOptions(new[] { "https://example.com/page/342" }, "example"))
        };        
}
