using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ResourcesSettings : NameOrRootUrlSettings
{
    public ResourcesSettings(bool debug, bool verbose, string nameOrRootUrl, string? name, string? rootUrl, string[]? pageUrls, bool onlyResourceLink) : base(debug, verbose, nameOrRootUrl, name, rootUrl)
    {
        PageUrls = pageUrls;
        OnlyResourceLink = onlyResourceLink;
    }

    [Description("Page URLs [bold][[pipeline]][/]")]
    [CommandOption("--pageUrls")]
    public string[]? PageUrls { get; }

    [Description("Output only the resource link instead of the format expected by 'scrap download'")]
    [CommandOption("--onlyResourceLink")]
    public bool OnlyResourceLink { get; }    
}
