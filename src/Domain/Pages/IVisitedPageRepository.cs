namespace Scrap.Domain.Pages;

public interface IVisitedPageRepository
{
    Task<bool> ExistsAsync(Uri uri) => Task.FromResult(false);
    IAsyncEnumerable<VisitedPage> GetAllAsync();
    Task UpsertAsync(Uri link);
    IAsyncEnumerable<VisitedPage> SearchAsync(string search);
    Task DeleteAsync(string search);
}
