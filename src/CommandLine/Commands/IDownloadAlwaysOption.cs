using CommandLine;

namespace Scrap.CommandLine.Commands;

internal interface IDownloadAlwaysOption
{
    [Option("downloadalways", Required = false, HelpText = "Download resources even if they are already downloaded")]
    bool DownloadAlways { get; }
}
