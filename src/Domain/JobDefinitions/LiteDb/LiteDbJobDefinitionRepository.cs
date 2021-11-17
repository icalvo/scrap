using System;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace Scrap.JobDefinitions.LiteDb
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
            _collection = _db.GetCollection<LiteDbJobDefinition>("JobDefinition");
            _collection.EnsureIndex("UniqueName", def => def.Name, unique: true);
        }

        public Task<JobDefinition?> GetByIdAsync(JobDefinitionId id)
        {
            return Task.FromResult(_collection.FindById((Guid)id)?.ToJobDefinition());
        }

        public Task<JobDefinition?> GetByNameAsync(string jobName)
        {
            var query = _collection.Query()
                .Where(x => x.Name == jobName);

            _logger.LogTrace("Query plan: {QueryPlan}", query.GetPlan());
            
            return Task.FromResult(query.SingleOrDefault()?.ToJobDefinition());
        }

        public Task UpsertAsync(JobDefinition jobDefinition)
        {
            try
            {
                _collection.Upsert((Guid)jobDefinition.Id, new LiteDbJobDefinition(jobDefinition));
            }
            catch (LiteException ex)
            {
                if (ex.ErrorCode == 110)
                {
                    throw new DuplicateNameException(ex.Message, ex);
                }
            }

            return Task.CompletedTask;
        }

        public Task<JobDefinition?> FindJobByRootUrlAsync(string rootUrl)
        {
            var query = _collection.Query()
                .Where(x => x.UrlPattern != null);

            _logger.LogTrace("Query plan: {QueryPlan}", query.GetPlan());
            var definitionsWithPatterns = query
                .ToEnumerable();
            
            return Task.FromResult(
                definitionsWithPatterns
                    .SingleOrDefault(x => Regex.IsMatch(rootUrl, x.UrlPattern!))
                    ?.ToJobDefinition(rootUrl));
        }

        public Task DeleteJobAsync(JobDefinitionId id)
        {
            _ = _collection.Delete(id.ToString());
            return Task.CompletedTask;
        }

        public Task<ImmutableArray<JobDefinition>> ListAsync()
        {
            return Task.FromResult(_collection.Query().OrderBy(x => x.Id).ToArray().Select(x => x.ToJobDefinition()).ToImmutableArray());
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}