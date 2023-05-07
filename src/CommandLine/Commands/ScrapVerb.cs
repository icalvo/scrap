using System.Diagnostics.CodeAnalysis;
using Scrap.Application.Scrap.One;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ScrapVerb : IVerb<ScrapVerb, ScrapOneOptions>
{
    private readonly IScrapOneApplicationService _scrapOneApplicationService;

    public ScrapVerb(IScrapOneApplicationService scrapOneApplicationService)
    {
        _scrapOneApplicationService = scrapOneApplicationService;
    }

    public async Task ExecuteAsync(ScrapOneOptions oneOptions) =>
        await _scrapOneApplicationService.ScrapAsync(oneOptions);
}
