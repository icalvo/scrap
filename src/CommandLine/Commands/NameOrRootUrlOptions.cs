using CommandLine;

namespace Scrap.CommandLine.Commands;

internal abstract class NameOrRootUrlOptions : OptionsBase
{
    [Value(0, HelpText = "Job definition name or root URL", MetaName = "Jobdef or root URL")]
    public string? NameOrRootUrl { get; set; }

    [Option('n', "name", Required = false, HelpText = "Job definition name")]
    public string? Name { get; set; }

    [Option('r', "rooturl", Required = false, HelpText = "Root URL")]
    public string? RootUrl { get; set; }
}
