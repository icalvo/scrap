using Microsoft.Extensions.Logging;

namespace Scrap.Domain.Pages;

public class LinkCalculator : ILinkCalculator
{
    private readonly ILogger<LinkCalculator> _logger;
    private readonly IVisitedPageRepository _visitedPageRepository;

    public LinkCalculator(ILogger<LinkCalculator> logger, IVisitedPageRepository visitedPageRepository)
    {
        _logger = logger;
        _visitedPageRepository = visitedPageRepository;
    }

    public async IAsyncEnumerable<Uri> CalculateLinks(IPage page, XPath? adjacencyXPath)
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
            if (await _visitedPageRepository.ExistsAsync(link))
            {
                _logger.LogDebug("Page {Link} already visited", link);
                continue;
            }

            yield return link;
        }
    }
}
