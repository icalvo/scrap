using System.ComponentModel;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

public abstract class SettingsBase : CommandSettings
{
    protected SettingsBase(bool debug, bool verbose)
    {
        Debug = debug;
        Verbose = verbose;
    }

    [Description("Debug")]
    [CommandOption("--dbg", IsHidden = true)]
    public bool Debug { get; }
    
    [Description("Verbose output")]
    [CommandOption("-v|--verbose")]
    public bool Verbose { get; }
}
