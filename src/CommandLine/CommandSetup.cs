using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Scrap.CommandLine.Commands;

namespace Scrap.CommandLine;

internal class CommandSetup<TCommand, TOptions> : ICommandSetup where TCommand : class, IVerb<TCommand, TOptions>
    where TOptions : OptionsBase
{
    private readonly IConfiguration _cfg;
    private readonly IServiceCollection _sc;

    public CommandSetup(IConfiguration cfg, IServiceCollection sc)
    {
        _cfg = cfg;
        _sc = sc;
    }

    public Type OptionsType => typeof(TOptions);

    public async Task ExecuteAsync(object options)
    {
        var typedOptions = (TOptions)options;
        var sc = new ServiceCollection { _sc };
        sc.AddSingleton<TCommand>();
        sc.AddLogging(_cfg, options);
        await using var sp = sc.BuildServiceProvider();
        if (options is OptionsBase { CheckGlobalConfig: true })
        {
            var checker = sp.GetRequiredService<IGlobalConfigurationChecker>();
            await checker.EnsureGlobalConfigurationAsync();
        }

        var command = sp.GetRequiredService<TCommand>();
        await command.ExecuteAsync(typedOptions);
    }
}
