using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrap.CommandLine.Commands;

namespace Scrap.CommandLine;

internal class NewCommandBuilder<TCommand, TOptions> : ICommandBuilder<TCommand, TOptions>
    where TCommand : class, ICommand<TCommand, TOptions>, new() where TOptions : OptionsBase
{
    public TCommand BuildCommand(IConfiguration cfg, IServiceCollection sc, TOptions o) => new();
}
