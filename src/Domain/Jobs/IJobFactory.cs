using Scrap.Domain.Jobs;

namespace Scrap.Domain;

public interface IJobFactory
{
    Task<Job> CreateAsync(JobDto jobDto);
}
