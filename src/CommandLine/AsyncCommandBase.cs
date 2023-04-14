using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

internal abstract class AsyncCommandBase<TSettings> : AsyncCommand<TSettings> where TSettings : SettingsBase
{
    protected readonly IConfiguration Configuration;
    private bool _verbose;

    protected AsyncCommandBase(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    
    public override async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
    {
        if (settings.Debug)
        {
            Debugger.Launch();
        }

        _verbose = settings.Verbose;

        var result = await CommandExecuteAsync(settings);
        
        if (settings.Debug)
        {
            Console.ReadKey();
        }

        return result;
    }
    protected abstract Task<int> CommandExecuteAsync(TSettings settings);
}
