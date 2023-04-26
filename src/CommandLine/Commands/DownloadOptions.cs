using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("download", aliases: new[] { "d" }, HelpText = "Downloads resources as given by the console input")]
internal sealed class DownloadOptions : NameOrRootUrlOptions
{
    public DownloadOptions(
        bool debug,
        bool verbose,
        string? nameOrRootUrlOption,
        string? nameOption,
        string? rootUrlOption,
        bool downloadAlways,
        string[]? resourceUrls) : base(debug, verbose, nameOrRootUrlOption, nameOption, rootUrlOption)
    {
        DownloadAlways = downloadAlways;
        ResourceUrls = resourceUrls;
    }

    [Option("downloadalways", Required = false, HelpText = "Download resources even if they are already downloaded")]
    public bool DownloadAlways { get; }

    [Option('f', "fullscan", Required = false, HelpText = "Resource URLs to download [bold][[pipeline]][/]")]
    public string[]? ResourceUrls { get; }

    public override bool ConsoleLog => true;
}
