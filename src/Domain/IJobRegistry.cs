using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;

namespace Scrap.Domain;

public interface IEntityRegistry<in T>
{
    void Register(T entity);
}
