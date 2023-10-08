using Scrap.Domain.Sites;
using SharpX;

namespace Scrap.Application;

public interface ICommandJobBuilder<TCommand, TJob>
{
    Task<Result<(TJob, Site), Unit>> Build(TCommand command);
}