using Scrap.Application.Scrap.One;

namespace Scrap.Application.Resources;

public interface IResourceCommand : ISingleScrapCommand
{
    Uri PageUrl { get; }
    int PageIndex { get; }
}
