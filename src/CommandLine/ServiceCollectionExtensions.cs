using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Scrap.CommandLine.Commands;
using Scrap.Infrastructure;

namespace Scrap.CommandLine;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLogging<TOptions>(
        this IServiceCollection sc,
        IConfiguration configuration,
        TOptions settings) =>
        sc.ConfigureLogging(
            configuration,
            settings is OptionsBase { ConsoleLog: true },
            settings is OptionsBase { Verbose: true });

    public static IServiceCollection Copy(this IServiceCollection sc) => new ServiceCollection { sc };
}
