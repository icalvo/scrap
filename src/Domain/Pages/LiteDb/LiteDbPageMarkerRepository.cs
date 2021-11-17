using System;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace Scrap.Pages.LiteDb
{
    public class LiteDbPageMarkerRepository : IPageMarkerRepository
    {
        private readonly ILogger<LiteDbPageMarkerRepository> _logger;
        private readonly bool _disableExists;
        private readonly ILiteCollection<PageMarker> _collection;

        public LiteDbPageMarkerRepository(ILiteDatabase db, ILogger<LiteDbPageMarkerRepository> logger, bool disableExists)
        {
            _logger = logger;
            _disableExists = disableExists;
            _collection = db.GetCollection<PageMarker>();
        }
        
        public Task<bool> ExistsAsync(Uri uri)
        {
            if (_disableExists)
            {
                return Task.FromResult(false);
            }

            var findById = _collection.FindById(uri.AbsoluteUri);
            return Task.FromResult(findById != null);
        }

        public Task AddAsync(Uri link)
        {
            _logger.LogTrace("Inserted {Page}", link.AbsoluteUri);
            if (_disableExists)
            {
                _collection.Upsert(link.AbsoluteUri, new PageMarker(link.AbsoluteUri));
            }
            else
            {
                _collection.Insert(link.AbsoluteUri, new PageMarker(link.AbsoluteUri));
            }

            return Task.CompletedTask;
        }
    }
}