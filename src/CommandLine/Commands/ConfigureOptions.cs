using System.Diagnostics.CodeAnalysis;
using CommandLine;
using Scrap.Domain;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("configure", aliases: new[] { "c", "cfg", "config" }, HelpText = "Configures the tool")]
public class ConfigureOptions : OptionsBase
{
    [Value(0, Required = false, HelpText = $"Config key (e.g. {ConfigKeys.BaseRootFolder})", MetaName = "KEY")]
    public string? Key { get; set; }

    [Value(1, Required = false, HelpText = "Value for the key", MetaName = "VALUE")]
    public string? Value { get; set; }

    public override bool ConsoleLog => true;
}
