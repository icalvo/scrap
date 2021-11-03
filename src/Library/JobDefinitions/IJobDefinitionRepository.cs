using System.Threading.Tasks;

namespace Scrap.JobDefinitions
{
    public interface IJobDefinitionRepository
    {
        Task<JobDefinition> GetByNameAsync(string jobName);
        Task AddAsync(string jobName, JobDefinition jobDefinition);
        Task<JobDefinition> FindJobByRootUrlAsync(string rootUrl);
        Task DeleteJobAsync(string jobName);
    }
}