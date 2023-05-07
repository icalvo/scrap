using Scrap.Domain.Pages;

namespace Scrap.Application.VisitedPages;

public class VisitedPagesApplicationService : IVisitedPagesApplicationService
{
    private readonly IVisitedPageRepository _visitedPageRepository;
    private readonly IVisitedPageRepositoryFactory _visitedPageRepositoryFactory;

    public VisitedPagesApplicationService(IVisitedPageRepositoryFactory visitedPageRepositoryFactory)
    {
        _visitedPageRepositoryFactory = visitedPageRepositoryFactory;
        _visitedPageRepository = visitedPageRepositoryFactory.Build();
    }

    public IAsyncEnumerable<Uri> SearchAsync(string search) =>
        _visitedPageRepository.SearchAsync(search).Select(x => new Uri(x.Uri));

    public Task DeleteAsync(string search) => _visitedPageRepository.DeleteAsync(search);

    public async Task MarkVisitedPageAsync(Uri pageUrl) => await _visitedPageRepository.UpsertAsync(pageUrl);

    public async Task MigrateAsync(DatabaseInfo dbInfo1, DatabaseInfo dbInfo2)
    {
        var repo1 = _visitedPageRepositoryFactory.Build(dbInfo1);
        var repo2 = _visitedPageRepositoryFactory.Build(dbInfo2);

        await foreach (var marker in repo1.GetAllAsync())
        {
            await repo2.UpsertAsync(new Uri(marker.Uri));
        }
    }
}
