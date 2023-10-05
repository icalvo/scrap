using Scrap.Domain;
using SharpX;

namespace Scrap.Application.Traversal;

public interface ITraverseCommand
{
    Maybe<NameOrRootUrl> NameOrRootUrl { get; }
    bool FullScan { get; }
}
