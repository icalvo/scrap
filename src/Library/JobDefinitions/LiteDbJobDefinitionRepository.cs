using System;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace Scrap.JobDefinitions
{
    public class LiteDbJobDefinitionRepository : IJobDefinitionRepository, IDisposable
    {
        private readonly ILiteDatabase _db;
        private readonly ILogger<LiteDbJobDefinitionRepository> _logger;
        private readonly ILiteCollection<LiteDbJobDefinition> _collection;

        public LiteDbJobDefinitionRepository(ILiteDatabase db, ILogger<LiteDbJobDefinitionRepository> logger)
        {
            _db = db;
            _logger = logger;
            _collection = _db.GetCollection<LiteDbJobDefinition>();
        }

        public Task<JobDefinition> GetByNameAsync(string jobName)
        {
            _logger.LogInformation("Getting job def. {JobName}", jobName);
            return Task.FromResult((JobDefinition)_collection.FindById(jobName));
        }

        public Task AddAsync(string jobName, JobDefinition jobDefinition)
        {
            _logger.LogInformation("Upserting job def. {JobName}", jobName);
            _collection.Upsert(jobName, new LiteDbJobDefinition(jobName, jobDefinition));
            return Task.CompletedTask;
        }

        public Task<JobDefinition> FindJobByRootUrlAsync(string rootUrl)
        {
            _logger.LogInformation("Getting job def. by URL {RootUrl}", rootUrl);
            var found = _collection.Query().Where(x => rootUrl.Contains(x.Id)).Single();
            var result = new JobDefinition(found, rootUrl);

            return Task.FromResult(result);
        }

        public Task DeleteJobAsync(string jobName)
        {
            _logger.LogTrace("Deleting job def. {JobName}", jobName);
            _ = _collection.Delete(jobName);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}