using LiteDB;
using Microsoft.Extensions.Logging;
using Scrap.Pages.LiteDb;

namespace Scrap.Pages
{
    public class PageMarkerRepositoryFactory
    {
        private readonly ILiteDatabase _db;
        private readonly ILoggerFactory _loggerFactory;

        public PageMarkerRepositoryFactory(ILiteDatabase db, ILoggerFactory loggerFactory)
        {
            _db = db;
            _loggerFactory = loggerFactory;
        }

        public IPageMarkerRepository Build(bool fullScan)
        {
            return new LiteDbPageMarkerRepository(
                _db,
                _loggerFactory.CreateLogger<LiteDbPageMarkerRepository>(),
                disableExists: fullScan);
        }
    }
}