namespace Scrap.JobDefinitions;

public interface IJobDefinitionRepository
{
    Task<JobDefinition?> GetByIdAsync(JobDefinitionId id);
    Task<JobDefinition?> GetByNameAsync(string jobName);
    Task<JobDefinition?> FindByRootUrlAsync(string rootUrl);
    Task UpsertAsync(JobDefinition jobDefinition);
    Task DeleteJobAsync(JobDefinitionId id);
    IAsyncEnumerable<JobDefinition> ListAsync();
}