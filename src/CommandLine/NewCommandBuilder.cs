using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrap.CommandLine.Commands;

namespace Scrap.CommandLine;

internal class NewCommandBuilder<TCommand, TOptions> : ICommandBuilder<TCommand, TOptions>
    where TCommand : class, IVerb<TCommand, TOptions>, new()
    where TOptions : OptionsBase
{
    public Task<TCommand> BuildCommandAsync(IConfiguration cfg, IServiceCollection sc, TOptions o) =>
        Task.FromResult(new TCommand());
}
