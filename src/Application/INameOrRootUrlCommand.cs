using Scrap.Domain;
using SharpX;

namespace Scrap.Application;

public interface INameOrRootUrlCommand {
    Maybe<NameOrRootUrl> NameOrRootUrl { get; }
}
