using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Scrap.Pages
{
    public class LinkedPagesCalculator : ILinkedPagesCalculator
    {
        private readonly IPageMarkerRepository _pageMarkerRepository;
        private readonly ILogger<LinkedPagesCalculator> _logger;

        public LinkedPagesCalculator(IPageMarkerRepository pageMarkerRepository, ILogger<LinkedPagesCalculator> logger)
        {
            _pageMarkerRepository = pageMarkerRepository;
            _logger = logger;
        }

        public async IAsyncEnumerable<Uri> GetLinkedPagesAsync(
            Page page,
            string adjacencyXPath,
            string adjacencyAttribute)
        {
            var links = page.Links(adjacencyXPath, adjacencyAttribute).ToArray();
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

                await _pageMarkerRepository.AddAsync(link);
                yield return link;
            }
        }        
    }
}
