using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrap.Application;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Infrastructure;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class SearchVisitedCommand : AsyncCommandBase<SearchSettings>
{
    public SearchVisitedCommand(IConfiguration configuration, IOAuthCodeGetter oAuthCodeGetter, IFileSystem fileSystem)
        : base(configuration, oAuthCodeGetter, fileSystem)
    {
    }

    protected override async Task<int> CommandExecuteAsync(SearchSettings settings)
    {
        var serviceResolver = BuildServiceProviderWithoutConsole();

        var visitedPagesAppService = serviceResolver.GetRequiredService<IVisitedPagesApplicationService>();
        var search = settings.search ?? ConsoleInput().First();
        var result = await visitedPagesAppService.SearchAsync(search);
        foreach (var line in result)
        {
            Console.WriteLine(line.Uri);
        }

        return 0;
    }
}
