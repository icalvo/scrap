using Scrap.Domain.Jobs;

namespace Scrap.Domain.Pages;

public interface IPageRetrieverFactory
{
    public IPageRetriever Build(IPageRetrieverOptions options);
}
