using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ServiceResolverCommand<TRawCommand, TSettings> : AsyncCommand<TSettings>
    where TRawCommand : class, ICommand<TSettings> where TSettings : SettingsBase
{
    private readonly IConfiguration _configuration;
    private readonly IServiceCollection _serviceCollection;

    public ServiceResolverCommand(IConfiguration configuration, IServiceCollection serviceCollection)
    {
        _configuration = configuration;
        _serviceCollection = serviceCollection;
    }

    public override Task<int> ExecuteAsync(CommandContext context, TSettings settings)
    {
        _serviceCollection.TryAddSingleton<TRawCommand>();
        var serviceResolver = ((CommandData)context.Data!).ConsoleLog
            ? _serviceCollection.BuildWithConsole(_configuration, settings)
            : _serviceCollection.BuildWithoutConsole(_configuration, settings);

        var rawCommand = serviceResolver.GetRequiredService<TRawCommand>();
        return rawCommand.Execute(context, settings);
    }
}
