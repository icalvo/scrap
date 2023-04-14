using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrap.Infrastructure;

namespace Scrap.CommandLine;

public static class ServiceProviderBuilder
{
    public static IServiceProvider BuildWithConsole(
        this IServiceCollection sc,
        IConfiguration configuration,
        SettingsBase settings) =>
        sc.ConfigureLogging(configuration, true, settings.Verbose).BuildServiceProvider(
            new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

    public static IServiceProvider BuildWithoutConsole(
        this IServiceCollection sc,
        IConfiguration configuration,
        SettingsBase settings) =>
        sc.ConfigureLogging(configuration, false, settings.Verbose).BuildServiceProvider(
            new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
}
