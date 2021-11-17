using System;
using System.Collections.Generic;

namespace Scrap.Pages
{
    public interface ILinkedPagesCalculator
    {
        IAsyncEnumerable<Uri> GetLinkedPagesAsync(
            Page page,
            string adjacencyXPath,
            string adjacencyAttribute,
            Uri baseUrl);
    }
}