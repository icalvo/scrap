using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class TraverseSettings : NameOrRootUrlSettings
{
    public TraverseSettings(bool debug, bool verbose, string nameOrRootUrl, string? name, string? rootUrl, bool fullScan) : base(debug, verbose, nameOrRootUrl, name, rootUrl)
    {
        FullScan = fullScan;
    }

    [Description("Navigate through already visited pages")]
    [CommandOption("-f|--fullScan")]
    public bool FullScan { get; }
}
