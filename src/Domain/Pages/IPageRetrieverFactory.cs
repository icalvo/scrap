using Scrap.Common;
using Scrap.Domain.Jobs;

namespace Scrap.Domain.Pages;

public interface IPageRetrieverFactory : IFactory<Job, IPageRetriever> {}