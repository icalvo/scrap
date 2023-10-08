using Scrap.Application.Download;
using Scrap.Domain;
using SharpX;

namespace Scrap.Application.Traversal;

public interface ITraverseCommand : INameOrRootUrlCommand
{
    bool FullScan { get; }
}
