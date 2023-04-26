using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrap.CommandLine.Commands;

namespace Scrap.CommandLine;

internal class CommandSetup<TCommand, TOptions> : ICommandSetup where TCommand : class, ICommand<TCommand, TOptions>
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

    public Task ExecuteAsync(object options) => ICommand<TCommand, TOptions>.ExecuteAsync(_cfg, _sc, (TOptions)options);
}
