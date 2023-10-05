using System.Diagnostics.CodeAnalysis;
using Scrap.Application.Traversal;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class TraverseVerb : IVerb<TraverseVerb, TraverseOptions>
{
    private readonly ITraversalApplicationService _traversalApplicationService;

    public TraverseVerb(ITraversalApplicationService traversalApplicationService)
    {
        _traversalApplicationService = traversalApplicationService;
    }

    public Task ExecuteAsync(TraverseOptions options) => _traversalApplicationService.TraverseAsync(options).ForEachAsync(x => Console.WriteLine(x));
}
