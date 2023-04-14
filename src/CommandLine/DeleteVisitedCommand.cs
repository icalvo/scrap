using System.Diagnostics.CodeAnalysis;
using Scrap.Application;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class DeleteVisitedCommand : AsyncCommand<SearchSettings>
{
    private readonly IVisitedPagesApplicationService _visitedPagesApplicationService;

    public DeleteVisitedCommand(IVisitedPagesApplicationService visitedPagesApplicationService)
    {
        _visitedPagesApplicationService = visitedPagesApplicationService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, SearchSettings settings)
    {
        var search = settings.search ?? ConsoleTools.ConsoleInput().First();
        var result = await _visitedPagesApplicationService.SearchAsync(search);
        foreach (var line in result)
        {
            Console.WriteLine(line.Uri);
        }

        Console.WriteLine();
        Console.WriteLine("Deleting...");
        await _visitedPagesApplicationService.DeleteAsync(search);
        Console.WriteLine("Finished!");
        return 0;
    }
}
