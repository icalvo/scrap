using Scrap.Domain.Pages;

namespace Scrap.Application;

public interface IDatabaseApplicationService
{
    Task<IEnumerable<PageMarker>> SearchAsync(string search);
    Task DeleteAsync(string search);
}
