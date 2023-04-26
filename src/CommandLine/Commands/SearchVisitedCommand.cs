using System.Diagnostics.CodeAnalysis;
using Scrap.Application;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class SearchVisitedCommand : ICommand<SearchVisitedCommand, SearchVisitedOptions>
{
    private readonly IVisitedPagesApplicationService _visitedPagesApplicationService;

    public SearchVisitedCommand(IVisitedPagesApplicationService visitedPagesApplicationService)
    {
        _visitedPagesApplicationService = visitedPagesApplicationService;
    }

    public async Task ExecuteAsync(SearchVisitedOptions options)
    {
        var search = options.Search ?? ConsoleTools.ConsoleInput().First();
        var result = await _visitedPagesApplicationService.SearchAsync(search);
        foreach (var line in result)
        {
            Console.WriteLine(line.Uri);
        }
    }
}
