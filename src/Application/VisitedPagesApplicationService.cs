using Scrap.Domain;
using Scrap.Domain.Pages;

namespace Scrap.Application;

public class VisitedPagesApplicationService : IVisitedPagesApplicationService
{
    private readonly IPageMarkerRepository _pageMarkerRepository;
    private readonly IPageMarkerRepositoryFactory _pageMarkerRepositoryFactory;

    public VisitedPagesApplicationService(IPageMarkerRepositoryFactory pageMarkerRepositoryFactory)
    {
        _pageMarkerRepositoryFactory = pageMarkerRepositoryFactory;
        _pageMarkerRepository = pageMarkerRepositoryFactory.Build();
    }

    public Task<IEnumerable<PageMarker>> SearchAsync(string search) => _pageMarkerRepository.SearchAsync(search);

    public Task DeleteAsync(string search) => _pageMarkerRepository.DeleteAsync(search);

    public async Task MarkVisitedPageAsync(Uri pageUrl) => await _pageMarkerRepository.UpsertAsync(pageUrl);

    public async Task MigrateAsync(DatabaseInfo dbInfo1, DatabaseInfo dbInfo2)
    {
        var repo1 = _pageMarkerRepositoryFactory.Build(dbInfo1);
        var repo2 = _pageMarkerRepositoryFactory.Build(dbInfo2);

        foreach (var marker in await repo1.GetAllAsync())
        {
            await repo2.UpsertAsync(new Uri(marker.Uri));
        }
    }
}
