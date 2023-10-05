namespace Scrap.Application.VisitedPages;

public interface IVisitedPagesApplicationService
{
    IAsyncEnumerable<Uri> SearchAsync(string search);
    Task DeleteAsync(string search);
    Task MarkVisitedPageAsync(Uri pageUrl);
}
