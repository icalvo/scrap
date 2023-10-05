using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb(
    "download",
    aliases: new[] { "d" },
    HelpText = "Downloads resources with format 'PAGE_INDEX PAGE_URL RES_INDEX RES_URL'")]
internal sealed class DownloadOptions : NameOrRootUrlOptions, IDownloadAlwaysOption
{
    public DownloadOptions(
        string? nameOrRootUrlOption = null,
        string? nameOption = null,
        string? rootUrlOption = null,
        string[]? lines = null,
        bool downloadAlways = false,
        bool debug = false,
        bool verbose = false) : base(debug, verbose, nameOrRootUrlOption, nameOption, rootUrlOption)
    {
        DownloadAlways = downloadAlways;
        ResourceLines = lines;
    }

    public bool DownloadAlways { get; }

    [Option('u', "urls", Required = false, HelpText = "Resource URLs to download [PIPELINE]")]
    public string[]? ResourceLines { get; }

    public override bool ConsoleLog => true;

    [Usage]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Called by CommandLineParser")]
    public static IEnumerable<Example> Examples =>
        new[]
        {
            new Example(
                "Downloads the image 'https://example.com/page/342/icon.gif' (which is the 5th resource in the page) from the page 'https://example.com/page/342' (which is the 3rd scrapped), as a resource of the site 'example'",
                new DownloadOptions(
                    nameOption: "example",
                    lines: new[] { "2 https://example.com/page/342 4 https://example.com/page/342/icon.gif" }))
        };    
}
