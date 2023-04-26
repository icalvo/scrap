using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Scrap.CommandLine.Commands;

internal interface ICommand<TCommand, TOptions> where TCommand : class, ICommand<TCommand, TOptions>
{
    static async Task<ICommand<TCommand, TOptions>> BuildCommand(
        IConfiguration cfg,
        IServiceCollection sc,
        TOptions options)
    {
        var cb = new ServiceProviderCommandBuilder<TCommand, TOptions>();
        return await cb.BuildCommandAsync(cfg, sc, options);
    }

    static async Task ExecuteAsync(IConfiguration cfg, IServiceCollection sc, TOptions options) =>
        await (await BuildCommand(cfg, sc, options)).ExecuteAsync(options);

    Task ExecuteAsync(TOptions settings);
}
