namespace Scrap.Domain.JobDefinitions;

public interface IJobDefinitionRepository
{
    Task<JobDefinition?> GetByNameAsync(string jobName);
    IAsyncEnumerable<JobDefinition> FindByRootUrlAsync(string rootUrl);
    IAsyncEnumerable<JobDefinition> ListAsync();
}
