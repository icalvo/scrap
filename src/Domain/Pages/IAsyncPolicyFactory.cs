using Polly;
using Scrap.Common;
using Scrap.Domain.Jobs;

namespace Scrap.Domain.Pages;

public interface IAsyncPolicyFactory : IFactory<Job, AsyncPolicyConfiguration, IAsyncPolicy> {}
