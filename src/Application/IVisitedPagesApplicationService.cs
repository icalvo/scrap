using Scrap.Domain.Pages;

namespace Scrap.Application;

public interface IVisitedPagesApplicationService
{
    Task<IEnumerable<PageMarker>> SearchAsync(string search);
    Task DeleteAsync(string search);
    Task MarkVisitedPageAsync(Uri pageUrl);
}
