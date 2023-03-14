namespace Scrap.Domain.Pages;

public interface IPageMarkerRepository
{
    Task<bool> ExistsAsync(Uri uri) => Task.FromResult(false);
    Task<IEnumerable<PageMarker>> GetAllAsync();
    Task UpsertAsync(Uri link);
    Task<IEnumerable<PageMarker>> SearchAsync(string search);
    Task DeleteAsync(string search);
}
