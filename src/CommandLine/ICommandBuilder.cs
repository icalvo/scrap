using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrap.CommandLine.Commands;

namespace Scrap.CommandLine;

internal interface ICommandBuilder<TCommand, TOptions> where TCommand : class, ICommand<TCommand, TOptions>
{
    TCommand BuildCommand(IConfiguration cfg, IServiceCollection sc, TOptions options);
}
