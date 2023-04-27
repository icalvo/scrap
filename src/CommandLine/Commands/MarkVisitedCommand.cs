using System.Diagnostics.CodeAnalysis;
using Scrap.Application;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class MarkVisitedCommand : ICommand<MarkVisitedCommand, MarkVisitedOptions>
{
    private readonly IVisitedPagesApplicationService _visitedPagesApplicationService;

    public MarkVisitedCommand(IVisitedPagesApplicationService visitedPagesApplicationService)
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
