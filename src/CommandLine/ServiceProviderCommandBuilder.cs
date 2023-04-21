using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrap.CommandLine.Commands;

namespace Scrap.CommandLine;

internal class ServiceProviderCommandBuilder<TCommand, TOptions> : ICommandBuilder<TCommand, TOptions>
    where TCommand : class, ICommand<TCommand, TOptions>
{
    public TCommand BuildCommand(IConfiguration cfg, IServiceCollection sc, TOptions options)
    {
        sc.AddSingleton<TCommand>();
        sc.AddLogging<TCommand, TOptions>(cfg, options);
        IServiceProvider sp = sc.BuildServiceProvider();
        return sp.GetRequiredService<TCommand>();
    }
}
