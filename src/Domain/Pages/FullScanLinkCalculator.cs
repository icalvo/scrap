using Microsoft.Extensions.Logging;
using Scrap.Domain.Pages;

namespace Scrap.Domain;

public class FullScanLinkCalculator : ILinkCalculator
{
    private readonly ILogger<FullScanLinkCalculator> _logger;

    public FullScanLinkCalculator(ILogger<FullScanLinkCalculator> logger)
    {
        _logger = logger;
    }

    public IAsyncEnumerable<Uri> CalculateLinks(
        IPage page,
        XPath? adjacencyXPath)
    {
        if (adjacencyXPath == null)
        {
            return Enumerable.Empty<Uri>().ToAsyncEnumerable();
        }

        var links = page.Links(adjacencyXPath).ToArray();
        if (links.Length == 0)
        {
            _logger.LogTrace("No links at {PageUri}", page.Uri);
            return Enumerable.Empty<Uri>().ToAsyncEnumerable();
        }

        return links.ToAsyncEnumerable();
    }
}