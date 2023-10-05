using Polly;
using Scrap.Domain.Jobs;

namespace Scrap.Domain.Pages;

public interface IAsyncPolicyFactory
{
    public IAsyncPolicy Build(IAsyncPolicyOptions options, AsyncPolicyConfiguration config);
}
