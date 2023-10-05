using Scrap.Domain.Jobs;

namespace Scrap.Domain.Pages;

public interface IVisitedPageRepositoryFactory
{
    public IVisitedPageRepository Build();
    public IVisitedPageRepository Build(IVisitedPageRepositoryOptions options);
    public IVisitedPageRepository Build(DatabaseInfo options);
}
