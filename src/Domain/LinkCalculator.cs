using Microsoft.Extensions.Logging;
using Scrap.Pages;

namespace Scrap;

public class LinkCalculator : ILinkCalculator
{
    private readonly ILogger<LinkCalculator> _logger;
    private readonly IPageMarkerRepository _pageMarkerRepository;

    public LinkCalculator(ILogger<LinkCalculator> logger, IPageMarkerRepository pageMarkerRepository)
    {
        _logger = logger;
        _pageMarkerRepository = pageMarkerRepository;
    }

    public async IAsyncEnumerable<Uri> CalculateLinks(
        IPage page,
        XPath? adjacencyXPath)
    {
        if (adjacencyXPath == null)
        {
            yield break;
        }

        var links = page.Links(adjacencyXPath).ToArray();
        if (links.Length == 0)
        {
            _logger.LogTrace("No links at {PageUri}", page.Uri);
            yield break;
        }

        foreach (var link in links)
        {
            if (await _pageMarkerRepository.ExistsAsync(link))
            {
                _logger.LogTrace("Page {Link} already visited", link);
                continue;
            }

            yield return link;
        }
    }
}

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
