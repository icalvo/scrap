using System.Diagnostics.CodeAnalysis;
using Scrap.Application.VisitedPages;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class DeleteVisitedVerb : IVerb<DeleteVisitedVerb, DeleteVisitedOptions>
{
    private readonly IVisitedPagesApplicationService _visitedPagesApplicationService;

    public DeleteVisitedVerb(IVisitedPagesApplicationService visitedPagesApplicationService)
    {
        _visitedPagesApplicationService = visitedPagesApplicationService;
    }

    public async Task ExecuteAsync(DeleteVisitedOptions options)
    {
        var search = options.Search ?? ConsoleTools.ConsoleInput().First();
        var result = _visitedPagesApplicationService.SearchAsync(search);
        await foreach (var uri in result)
        {
            Console.WriteLine(uri);
        }

        Console.WriteLine();
        Console.WriteLine("Deleting...");
        await _visitedPagesApplicationService.DeleteAsync(search);
        Console.WriteLine("Finished!");
    }
}
