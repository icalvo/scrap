using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;

namespace Scrap.Application;

public class MarkVisitedApplicationService : IMarkVisitedApplicationService
{
    private readonly IJobFactory _jobFactory;
    private readonly IPageMarkerRepository _pageMarkerRepository;

    public MarkVisitedApplicationService(IJobFactory jobFactory, IPageMarkerRepository pageMarkerRepository)
    {
        _jobFactory = jobFactory;
        _pageMarkerRepository = pageMarkerRepository;
    }

    public async Task MarkVisitedPageAsync(JobDto jobDto, Uri pageUrl)
    {
        var _ = await _jobFactory.CreateAsync(jobDto);
        
        await _pageMarkerRepository.UpsertAsync(pageUrl);
    }
    
}
