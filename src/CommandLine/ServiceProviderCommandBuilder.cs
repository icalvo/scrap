using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrap.CommandLine.Commands;

namespace Scrap.CommandLine;

internal class ServiceProviderCommandBuilder<TCommand, TOptions> : ICommandBuilder<TCommand, TOptions>
    where TCommand : class, ICommand<TCommand, TOptions>
{
    public async Task<TCommand> BuildCommandAsync(IConfiguration cfg, IServiceCollection sc, TOptions options)
    {
        sc.AddSingleton<TCommand>();
        sc.AddLogging(cfg, options);
        IServiceProvider sp = sc.BuildServiceProvider();
        if (options is OptionsBase { CheckGlobalConfig: true })
        {
            var checker = sp.GetRequiredService<IGlobalConfigurationChecker>();
            await checker.EnsureGlobalConfigurationAsync();
        }

        return sp.GetRequiredService<TCommand>();
    }
}
