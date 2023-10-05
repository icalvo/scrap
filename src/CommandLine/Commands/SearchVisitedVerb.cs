using System.Diagnostics.CodeAnalysis;
using Scrap.Application.VisitedPages;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class SearchVisitedVerb : IVerb<SearchVisitedVerb, SearchVisitedOptions>
{
    private readonly IVisitedPagesApplicationService _visitedPagesApplicationService;

    public SearchVisitedVerb(IVisitedPagesApplicationService visitedPagesApplicationService)
    {
        _visitedPagesApplicationService = visitedPagesApplicationService;
    }

    public async Task ExecuteAsync(SearchVisitedOptions options)
    {
        var search = options.Search ?? ConsoleTools.ConsoleInput().First();
        var result = _visitedPagesApplicationService.SearchAsync(search);
        await foreach (var uri in result)
        {
            Console.WriteLine(uri);
        }
    }
}
