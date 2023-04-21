using Scrap.Domain.JobDefinitions;

namespace Scrap.Application;

public interface IJobDefinitionsApplicationService
{
    Task<JobDefinitionDto?> FindByNameAsync(string name);
    IAsyncEnumerable<JobDefinitionDto> GetAllAsync();
    IAsyncEnumerable<JobDefinitionDto> FindByRootUrlAsync(string rootUrl);
}
