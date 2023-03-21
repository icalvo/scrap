using System.ComponentModel;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

public abstract class ScrapSettingsBase : SettingsBase
{
    protected ScrapSettingsBase(bool debug, bool verbose, bool fullScan, bool downloadAlways, bool disableMarkingVisited, bool disableResourceWrites) : base(debug, verbose)
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
    [CommandOption("--dmv|--disableMarkingVisited")]
    public bool DisableMarkingVisited { get; }

    [Description("Disable writing the resource")]
    [CommandOption("--drw|--disableResourceWrite")]
    public bool DisableResourceWrites { get; }
}