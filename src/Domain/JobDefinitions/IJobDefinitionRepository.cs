using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Scrap.JobDefinitions
{
    public interface IJobDefinitionRepository
    {
        Task<JobDefinition?> GetByIdAsync(JobDefinitionId id);
        Task<JobDefinition?> GetByNameAsync(string jobName);
        Task<JobDefinition?> FindByRootUrlAsync(string rootUrl);
        Task UpsertAsync(JobDefinition jobDefinition);
        Task DeleteJobAsync(JobDefinitionId id);
        Task<ImmutableArray<JobDefinition>> ListAsync();
    }
}