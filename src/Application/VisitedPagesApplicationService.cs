using Microsoft.Extensions.Logging;
using Scrap.Common;
using Scrap.Domain;
using Scrap.Domain.Pages;

namespace Scrap.Application;

public class VisitedPagesApplicationService : IVisitedPagesApplicationService
{
    private readonly IPageMarkerRepository _pageMarkerRepository;
    private readonly ILoggerFactory _loggerFactory;

    public VisitedPagesApplicationService(IFactory<IPageMarkerRepository> pageMarkerRepositoryFactory, ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _pageMarkerRepository = pageMarkerRepositoryFactory.Build();
    }

    public Task<IEnumerable<PageMarker>> SearchAsync(string search) => _pageMarkerRepository.SearchAsync(search);

    public Task DeleteAsync(string search) => _pageMarkerRepository.DeleteAsync(search);

    public async Task MarkVisitedPageAsync(Uri pageUrl) => await _pageMarkerRepository.UpsertAsync(pageUrl);

    public async Task MigrateAsync(DatabaseInfo dbInfo1, DatabaseInfo dbInfo2)
    {
        var repo1 = new PageMarkerRepositoryFactory(
            dbInfo1, _loggerFactory.CreateLogger<PageMarkerRepositoryFactory>(), _loggerFactory).Build();
        var repo2 = new PageMarkerRepositoryFactory(dbInfo2, _loggerFactory.CreateLogger<PageMarkerRepositoryFactory>(), _loggerFactory).Build();

        foreach (var marker in await repo1.GetAllAsync())
        {
            await repo2.UpsertAsync(new Uri(marker.Uri));
        }
    }
}
