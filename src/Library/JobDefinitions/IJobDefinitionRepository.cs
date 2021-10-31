using System.Threading.Tasks;

namespace Scrap.JobDefinitions
{
    public interface IJobDefinitionRepository
    {
        Task<ScrapJobDefinition> GetByNameAsync(string jobName);
        Task AddAsync(string jobName, ScrapJobDefinition scrapJobDefinition);
    }
}