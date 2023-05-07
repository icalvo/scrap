using Scrap.Application.Scrap.All;
using Scrap.Domain;
using SharpX;

namespace Scrap.Application.Scrap.One;

public interface IScrapOneCommand : IScrapAllCommand
{
    Maybe<NameOrRootUrl> NameOrRootUrl { get; }
}
