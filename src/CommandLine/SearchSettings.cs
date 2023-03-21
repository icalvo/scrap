using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class SearchSettings : SettingsBase
{
    public SearchSettings(bool debug, bool verbose, string? search) : base(debug, verbose)
    {
        this.search = search;
    }

    [Description("Search with Regular Expression [bold][[pipeline]][/]")]
    [CommandArgument(0, "<search>")]
    public string? search { get; }
}
