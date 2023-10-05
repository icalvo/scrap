using System.Diagnostics.CodeAnalysis;
using Scrap.Application.Scrap.All;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class AllVerb : IVerb<AllVerb, ScrapAllOptions>
{
    private readonly IGlobalConfigurationChecker _checker;
    private readonly IScrapAllApplicationService _scrapAllApplicationService;

    public AllVerb(IGlobalConfigurationChecker checker, IScrapAllApplicationService scrapAllApplicationService)
    {
        _checker = checker;
        _scrapAllApplicationService = scrapAllApplicationService;
    }

    public async Task ExecuteAsync(ScrapAllOptions options)
    {
        await _checker.EnsureGlobalConfigurationAsync();
        await _scrapAllApplicationService.ScrapAllAsync(options);
    }
}
