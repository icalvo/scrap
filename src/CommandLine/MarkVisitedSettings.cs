using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class MarkVisitedSettings : SettingsBase
{
    public MarkVisitedSettings(bool debug, bool verbose, string[]? url) : base(debug, verbose)
    {
        this.Url = url;
    }
    [Description("URL [bold][[pipeline]][/]")]
    [CommandOption("-u|--url")]
    public string[]? Url { get; }
}
