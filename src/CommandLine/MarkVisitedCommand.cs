using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrap.Application;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Infrastructure;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class MarkVisitedCommand : AsyncCommandBase<MarkVisitedSettings>
{
    public MarkVisitedCommand(IConfiguration configuration, IOAuthCodeGetter oAuthCodeGetter, IFileSystem fileSystem)
        : base(configuration, oAuthCodeGetter, fileSystem)
    {
    }

    protected override async Task<int> CommandExecuteAsync(MarkVisitedSettings settings)
    {
        var serviceResolver = BuildServiceProviderWithoutConsole();

        var visitedPagesAppService = serviceResolver.GetRequiredService<IVisitedPagesApplicationService>();
        var inputLines = settings.Url ?? ConsoleInput();
        foreach (var line in inputLines)
        {
            var pageUrl = new Uri(line);
            await visitedPagesAppService.MarkVisitedPageAsync(pageUrl);
            Console.WriteLine($"Visited {pageUrl}");
        }

        return 0;
    }
}
