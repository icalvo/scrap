using System.Collections.Immutable;
using System.Threading.Tasks;
using Scrap.JobDefinitions;

namespace Scrap.Jobs
{
    public interface IJobRepository
    {
        Task AddAsync(Job job);
        Task<Job?> GetByIdAsync(JobId id);
        Task DeleteJobAsync(JobDefinitionId id);
        Task<ImmutableArray<Job>> ListAsync();
    }
}