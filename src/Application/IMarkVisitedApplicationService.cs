using Scrap.Domain.Jobs;

namespace Scrap.Application;

public interface IMarkVisitedApplicationService
{
    Task MarkVisitedPageAsync(NewJobDto jobDto, Uri pageUrl);
}
