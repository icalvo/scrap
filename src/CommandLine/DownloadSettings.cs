using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class DownloadSettings : NameOrRootUrlSettings
{
    public DownloadSettings(
        bool debug,
        bool verbose,
        string nameOrRootUrl,
        string? name,
        string? rootUrl,
        bool downloadAlways,
        string[]? resourceUrls) : base(debug, verbose, nameOrRootUrl, name, rootUrl)
    {
        DownloadAlways = downloadAlways;
        ResourceUrls = resourceUrls;
    }

    [Description("Download resources even if they are already downloaded")]
    [CommandOption("-d|--downloadalways")]
    public bool DownloadAlways { get; }

    [Description("Resource URLs to download [bold][[pipeline]][/]")]
    [CommandOption("--resourceurls")]
    public string[]? ResourceUrls { get; }
}
