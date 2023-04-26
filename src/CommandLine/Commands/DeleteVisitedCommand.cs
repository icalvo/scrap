using System.Diagnostics.CodeAnalysis;
using Scrap.Application;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class DeleteVisitedCommand : ICommand<DeleteVisitedCommand, DeleteVisitedOptions>
{
    private readonly IVisitedPagesApplicationService _visitedPagesApplicationService;

    public DeleteVisitedCommand(IVisitedPagesApplicationService visitedPagesApplicationService)
    {
        _visitedPagesApplicationService = visitedPagesApplicationService;
    }

    public async Task ExecuteAsync(DeleteVisitedOptions options)
    {
        var search = options.Search ?? ConsoleTools.ConsoleInput().First();
        var result = await _visitedPagesApplicationService.SearchAsync(search);
        foreach (var line in result)
        {
            Console.WriteLine(line.Uri);
        }

        Console.WriteLine();
        Console.WriteLine("Deleting...");
        await _visitedPagesApplicationService.DeleteAsync(search);
        Console.WriteLine("Finished!");
    }
}
