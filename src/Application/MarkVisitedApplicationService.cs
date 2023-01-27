using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;

namespace Scrap.Application;

public class MarkVisitedApplicationService : IMarkVisitedApplicationService
{
    private readonly IAsyncFactory<JobDto, Job> _jobFactory;
    private readonly IFactory<Job, IPageMarkerRepository> _pageMarkerRepositoryFactory;

    public MarkVisitedApplicationService(
        IAsyncFactory<JobDto, Job> jobFactory,
        IFactory<Job, IPageMarkerRepository> pageMarkerRepositoryFactory)
    {
        _jobFactory = jobFactory;
        _pageMarkerRepositoryFactory = pageMarkerRepositoryFactory;
    }

    public async Task MarkVisitedPageAsync(JobDto jobDto, Uri pageUrl)
    {
        var job = await _jobFactory.Build(jobDto);
        var pageMarkerRepository = _pageMarkerRepositoryFactory.Build(job);
        await pageMarkerRepository.UpsertAsync(pageUrl);
    }
    
}
