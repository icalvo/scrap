using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Scrap.Domain;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class ConfigureSettings : SettingsBase
{
    public ConfigureSettings(bool debug, bool verbose, string? key, string? value) : base(debug, verbose)
    {
        this.Key = key;
        this.Value = value;
    }

    [Description($"Config key (e.g. {ConfigKeys.BaseRootFolder})")]
    [CommandArgument(0, "[key]")]
    public string? Key { get; }

    [Description("Value for the key")]
    [CommandArgument(1, "[value]")]
    public string? Value { get; }
}
