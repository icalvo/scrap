using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Scrap.CommandLine.Commands;

internal interface ICommand<TCommand, TOptions> where TCommand : class, ICommand<TCommand, TOptions>
{
    static ICommand<TCommand, TOptions> BuildCommand(IConfiguration cfg, IServiceCollection sc, TOptions options)
    {
        var cb = new ServiceProviderCommandBuilder<TCommand, TOptions>();
        return cb.BuildCommand(cfg, sc, options);
    }

    static Task ExecuteAsync(IConfiguration cfg, IServiceCollection sc, TOptions options) =>
        BuildCommand(cfg, sc, options).ExecuteAsync(options);

    Task ExecuteAsync(TOptions settings);
}
