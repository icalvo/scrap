using Scrap.Application.Scrap.One;

namespace Scrap.Application.Resources;

public interface IResourceCommand : IScrapOneCommand
{
    Uri PageUrl { get; }
    int PageIndex { get; }
}
