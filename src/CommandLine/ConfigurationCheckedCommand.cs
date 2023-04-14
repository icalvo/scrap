using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal class ConfigurationCheckedCommand<TRawCommand, TSettings> : AsyncCommand<TSettings>
    where TRawCommand : class, ICommand<TSettings> where TSettings : SettingsBase
{
    private readonly TRawCommand _commandImplementation;
    private readonly IGlobalConfigurationChecker _globalConfigurationChecker;

    public ConfigurationCheckedCommand(
        TRawCommand commandImplementation,
        IGlobalConfigurationChecker globalConfigurationChecker)
    {
        _commandImplementation = commandImplementation;
        _globalConfigurationChecker = globalConfigurationChecker;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
    {
        await _globalConfigurationChecker.EnsureGlobalConfigurationAsync();
        return await _commandImplementation.Execute(context, settings);
    }
}

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal class ConfigurationCheckedCommand<TSettings> : AsyncCommand<TSettings> where TSettings : SettingsBase
{
    private readonly ICommand<TSettings> _commandImplementation;
    private readonly IGlobalConfigurationChecker _globalConfigurationChecker;

    public ConfigurationCheckedCommand(
        ICommand<TSettings> commandImplementation,
        IGlobalConfigurationChecker globalConfigurationChecker)
    {
        _commandImplementation = commandImplementation;
        _globalConfigurationChecker = globalConfigurationChecker;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
    {
        await _globalConfigurationChecker.EnsureGlobalConfigurationAsync();
        return await _commandImplementation.Execute(context, settings);
    }
}
