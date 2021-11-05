using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Scrap.JobDefinitions
{
    public interface IJobDefinitionRepository
    {
        Task<JobDefinition> GetByNameAsync(string jobName);
        Task AddAsync(string jobName, JobDefinition jobDefinition);
        Task<JobDefinition> FindJobByRootUrlAsync(string rootUrl);
        Task DeleteJobAsync(string jobName);
        Task<ImmutableArray<JobDefinition>> ListAsync();
    }
}