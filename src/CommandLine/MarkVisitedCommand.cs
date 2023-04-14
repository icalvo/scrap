using System.Diagnostics.CodeAnalysis;
using Scrap.Application;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class MarkVisitedCommand : AsyncCommand<MarkVisitedSettings>
{
    private readonly IVisitedPagesApplicationService _visitedPagesApplicationService;

    public MarkVisitedCommand(IVisitedPagesApplicationService visitedPagesApplicationService)
    {
        _visitedPagesApplicationService = visitedPagesApplicationService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, MarkVisitedSettings settings)
    {
        var inputLines = settings.Url ?? ConsoleTools.ConsoleInput();
        foreach (var line in inputLines)
        {
            var pageUrl = new Uri(line);
            await _visitedPagesApplicationService.MarkVisitedPageAsync(pageUrl);
            Console.WriteLine($"Visited {pageUrl}");
        }

        return 0;
    }
}
