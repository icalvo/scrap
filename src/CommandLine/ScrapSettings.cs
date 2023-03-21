using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ScrapSettings : NameOrRootUrlSettings, IScrapSettings
{
    public ScrapSettings(bool debug, bool verbose, string nameOrRootUrl, string? name, string? rootUrl, bool fullScan, bool downloadAlways, bool disableMarkingVisited, bool disableResourceWrites) : base(debug, verbose, nameOrRootUrl, name, rootUrl)
    {
        FullScan = fullScan;
        DownloadAlways = downloadAlways;
        DisableMarkingVisited = disableMarkingVisited;
        DisableResourceWrites = disableResourceWrites;
    }

    [Description("Navigate through already visited pages")]
    [CommandOption("-f|--fullScan")]
    public bool FullScan { get; }

    [Description("Download resources even if they are already downloaded")]
    [CommandOption("-d|--downloadAlways")]
    public bool DownloadAlways { get; }

    [Description("Disable mark as visited")]
    [CommandOption("--disableMarkingVisited|--dmv")]
    public bool DisableMarkingVisited { get; }

    [Description("Disable writing the resource")]
    [CommandOption("--drw|--disableResourceWrite")]
    public bool DisableResourceWrites { get; }
}
