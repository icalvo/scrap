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
        var inputLines = options.Url ?? ConsoleTools.ConsoleInput();
        foreach (var line in inputLines)
        {
            var pageUrl = new Uri(line);
            await _visitedPagesApplicationService.MarkVisitedPageAsync(pageUrl);
            Console.WriteLine($"Visited {pageUrl}");
        }
    }
}
