using Scrap.Common;
using Scrap.Domain.Jobs;

namespace Scrap.Domain.Pages;

public interface IVisitedPageRepositoryFactory
    : IOptionalParameterFactory<Job, IVisitedPageRepository>, IFactory<DatabaseInfo, IVisitedPageRepository>
{
}
