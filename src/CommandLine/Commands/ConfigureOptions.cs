using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;
using Scrap.Domain;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("configure", aliases: new[] { "c", "cfg", "config" }, HelpText = "Configures the tool")]
public class ConfigureOptions : OptionsBase
{
    public ConfigureOptions(string? key = null, string? value = null, bool debug = false, bool verbose = false) : base(
        debug,
        verbose)
    {
        Key = key;
        Value = value;
    }

    [Value(0, Required = false, HelpText = $"Config key (e.g. {ConfigKeys.BaseRootFolder})", MetaName = "KEY")]
    public string? Key { get; }

    [Value(1, Required = false, HelpText = "Value for the key", MetaName = "VALUE")]
    public string? Value { get; }

    public override bool ConsoleLog => true;


    [Usage]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Called by CommandLineParser")]
    public static IEnumerable<Example> Examples =>
        new[]
        {
            new Example("Sets all config interactively", new ConfigureOptions()),
            new Example(
                "Sets the base download path",
                new ConfigureOptions(ConfigKeys.BaseRootFolder, "C:\\downloads"))
        };
}
