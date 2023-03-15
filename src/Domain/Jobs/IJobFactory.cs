using Scrap.Common;

namespace Scrap.Domain.Jobs;

public interface IJobFactory : IAsyncFactory<JobDto, Job> {}
