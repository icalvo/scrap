using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrap.CommandLine.Commands;

namespace Scrap.CommandLine;

internal class CommandSetup<TCommand, TOptions> : ICommandSetup where TCommand : class, IVerb<TCommand, TOptions>
    where TOptions : OptionsBase
{
    private readonly IConfiguration _cfg;
    private readonly IServiceCollection _baseServiceCollection;

    public CommandSetup(IConfiguration cfg, IServiceCollection baseServiceCollection)
    {
        _cfg = cfg;
        _baseServiceCollection = baseServiceCollection;
    }

    public Type OptionsType => typeof(TOptions);

    public async Task ExecuteAsync(object options)
    {
        var typedOptions = (TOptions)options;
        var commandServiceCollection = _baseServiceCollection.Copy();
        commandServiceCollection.AddSingleton<TCommand>();
        commandServiceCollection.AddLogging(_cfg, options);
        await using var sp = commandServiceCollection.BuildServiceProvider();
        if (options is OptionsBase { CheckGlobalConfig: true })
        {
            var checker = sp.GetRequiredService<IGlobalConfigurationChecker>();
            await checker.EnsureGlobalConfigurationAsync();
        }

        var command = sp.GetRequiredService<TCommand>();
        await command.ExecuteAsync(typedOptions);
    }
}
