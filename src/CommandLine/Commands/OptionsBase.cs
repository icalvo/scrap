using CommandLine;

namespace Scrap.CommandLine.Commands;

public abstract class OptionsBase
{
    [Option('d', "dbg", Required = false, Hidden = true, HelpText = "Debug")]
    public bool Debug { get; set; }

    [Option('v', "verbose", Required = false, HelpText = "Verbose output")]
    public bool Verbose { get; set; }

    public abstract bool ConsoleLog { get; }
}
