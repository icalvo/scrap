using Scrap.Domain;
using Scrap.Domain.Pages;

namespace Scrap.Application;

public class VisitedPagesApplicationService : IVisitedPagesApplicationService
{
    private readonly IPageMarkerRepository _pageMarkerRepository;

    public VisitedPagesApplicationService(IFactory<IPageMarkerRepository> pageMarkerRepositoryFactory)
    {
        _pageMarkerRepository = pageMarkerRepositoryFactory.Build();
    }

    public Task<IEnumerable<PageMarker>> SearchAsync(string search)
    {
        return _pageMarkerRepository.SearchAsync(search);
    }

    public Task DeleteAsync(string search)
    {
        return _pageMarkerRepository.DeleteAsync(search);
    }

    public async Task MarkVisitedPageAsync(Uri pageUrl)
    {
        await _pageMarkerRepository.UpsertAsync(pageUrl);
    }
}
