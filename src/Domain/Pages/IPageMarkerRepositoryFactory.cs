using Scrap.Common;
using Scrap.Domain.Jobs;

namespace Scrap.Domain.Pages;

public interface IPageMarkerRepositoryFactory :
    IOptionalParameterFactory<Job, IPageMarkerRepository>,
    IFactory<DatabaseInfo, IPageMarkerRepository> { }
