using System.Diagnostics.CodeAnalysis;
using Scrap.Application.Scrap.One;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ScrapVerb : IVerb<ScrapVerb, SingleScrapOptions>
{
    private readonly ISingleScrapApplicationService _singleScrapApplicationService;

    public ScrapVerb(ISingleScrapApplicationService singleScrapApplicationService)
    {
        _singleScrapApplicationService = singleScrapApplicationService;
    }

    public async Task ExecuteAsync(SingleScrapOptions options) =>
        await _singleScrapApplicationService.ScrapAsync(options);
}
