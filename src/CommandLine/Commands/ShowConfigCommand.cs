using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ShowConfigCommand : ICommand<ShowConfigCommand, ShowConfigOptions>
{
    private readonly IConfiguration _configuration;

    public ShowConfigCommand(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<int> ExecuteAsync(ShowConfigOptions options)
    {
        Debug.Assert(_configuration != null, nameof(_configuration) + " != null");
        var root = (IConfigurationRoot)_configuration;
        Console.WriteLine(root.GetDebugView());

        return Task.FromResult(0);
    }
}
