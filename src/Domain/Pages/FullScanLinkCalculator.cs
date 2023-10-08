using Microsoft.Extensions.Logging;
using Scrap.Common;
using SharpX;

namespace Scrap.Domain.Pages;

public class FullScanLinkCalculator : ILinkCalculator
{
    private readonly ILogger<FullScanLinkCalculator> _logger;

    public FullScanLinkCalculator(ILogger<FullScanLinkCalculator> logger)
    {
        _logger = logger;
    }

    public IAsyncEnumerable<Uri> CalculateLinks(IPage page, Maybe<XPath> adjacencyXPath) =>
        adjacencyXPath
            .Select(page.Links)
            .Select(x => x.ToArray())
            .Select(x =>
            {
                if (x.Length == 0)
                {
                    _logger.LogTrace("No links at {PageUri}", page.Uri);;
                }

                return x;
            })
            .Select(x => x.ToAsyncEnumerable())
            .FromJust2(AsyncEnumerable.Empty<Uri>());
}
