using System;
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

        public Task<ScrapJobDefinition> GetByNameAsync(string jobName)
        {
            _logger.LogInformation("Getting job def. {JobName}", jobName);
            return Task.FromResult((ScrapJobDefinition)_collection.FindById(jobName));
        }

        public Task AddAsync(string jobName, ScrapJobDefinition scrapJobDefinition)
        {
            _logger.LogInformation("Upserting job def. {JobName}", jobName);
            _collection.Upsert(jobName, new LiteDbJobDefinition(jobName, scrapJobDefinition));
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}