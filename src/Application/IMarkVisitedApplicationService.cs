using Scrap.Domain.Jobs;

namespace Scrap.Application;

public interface IMarkVisitedApplicationService
{
    Task MarkVisitedPageAsync(JobDto jobDto, Uri pageUrl);
}
