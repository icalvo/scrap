using System.Diagnostics.CodeAnalysis;
using Scrap.Application.VisitedPages;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class MarkVisitedVerb : IVerb<MarkVisitedVerb, MarkVisitedOptions>
{
    private readonly IVisitedPagesApplicationService _visitedPagesApplicationService;

    public MarkVisitedVerb(IVisitedPagesApplicationService visitedPagesApplicationService)
    {
        _visitedPagesApplicationService = visitedPagesApplicationService;
    }

    public async Task ExecuteAsync(MarkVisitedOptions options)
    {
        var urlLines = options.Urls ?? ConsoleTools.ConsoleInput();
        foreach (var urlLine in urlLines)
        {
            var pageUrl = new Uri(urlLine);
            await _visitedPagesApplicationService.MarkVisitedPageAsync(pageUrl);
            Console.WriteLine($"Visited {pageUrl}");
        }
    }
}
