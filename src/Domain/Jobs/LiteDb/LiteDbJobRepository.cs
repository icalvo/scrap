using System;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Logging;
using Scrap.JobDefinitions;

namespace Scrap.Jobs.LiteDb
{
    public class LiteDbJobRepository : IJobRepository, IDisposable
    {
        private readonly ILiteDatabase _db;
        private readonly ILogger<LiteDbJobRepository> _logger;
        private readonly ILiteCollection<LiteDbJob> _collection;

        public LiteDbJobRepository(ILiteDatabase db, ILogger<LiteDbJobRepository> logger)
        {
            _db = db;
            _logger = logger;
            _collection = _db.GetCollection<LiteDbJob>("Job");
        }

        public Task<Job?> GetByIdAsync(JobId id)
        {
            return Task.FromResult(_collection.FindById((Guid)id)?.ToJob());
        }

        public Task AddAsync(Job job)
        {
            try
            {
                _collection.Upsert((Guid)job.Id, new LiteDbJob(job));
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

        public Task DeleteJobAsync(JobDefinitionId id)
        {
            _ = _collection.Delete(id.ToString());
            return Task.CompletedTask;
        }

        public Task<ImmutableArray<Job>> ListAsync()
        {
            return Task.FromResult(_collection.Query().OrderBy(x => x.Id).ToArray().Select(x => x.ToJob()).ToImmutableArray());
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}