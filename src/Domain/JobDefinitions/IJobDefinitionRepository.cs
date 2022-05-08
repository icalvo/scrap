namespace Scrap.Domain.JobDefinitions;

public interface IJobDefinitionRepository
{
    Task<JobDefinition?> GetByIdAsync(JobDefinitionId id);
    Task<JobDefinition?> GetByNameAsync(string jobName);
    IAsyncEnumerable<JobDefinition> FindByRootUrlAsync(string rootUrl);
    Task UpsertAsync(JobDefinition jobDefinition);
    Task DeleteJobAsync(JobDefinitionId id);
    IAsyncEnumerable<JobDefinition> ListAsync();
}
