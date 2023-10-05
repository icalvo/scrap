using CommandLine;

namespace Scrap.CommandLine.Commands;

public abstract class OptionsBase
{
    public const char VerboseLetter = 'v';
    protected OptionsBase(bool debug, bool verbose)
    {
        Debug = debug;
        Verbose = verbose;
    }

    [Option('d', "dbg", Required = false, Hidden = true, HelpText = "Debug")]
    public bool Debug { get; }

    [Option(VerboseLetter, "verbose", Required = false, HelpText = "Verbose output")]
    public bool Verbose { get; }

    public abstract bool ConsoleLog { get; }

    public virtual bool CheckGlobalConfig => true;
}
