using Microsoft.Extensions.Logging;
using Scrap.Common;
using SharpX;

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
            .Select(x =>
                x.ToAsyncEnumerable()
                    .SelectAwait(
                        async link => (link, visited: await _visitedPageRepository.ExistsAsync(link))
                    )
                    .DoIf(
                        tuple => tuple.visited,
                        tuple => _logger.LogDebug("Page {Link} already visited", tuple.link))
                    .Where(tuple => !tuple.visited)
                    .Select(tuple => tuple.link))
            .FromJust2(AsyncEnumerable.Empty<Uri>());
}
