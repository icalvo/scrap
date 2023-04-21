using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrap.CommandLine.Commands;
using Scrap.Infrastructure;

namespace Scrap.CommandLine;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLogging<TCommand, TOptions>(
        this IServiceCollection sc,
        IConfiguration configuration,
        TOptions settings) where TCommand : class, ICommand<TCommand, TOptions> =>
        sc.ConfigureLogging(
            configuration,
            settings is OptionsBase { ConsoleLog: true },
            settings is OptionsBase { Verbose: true });
}
