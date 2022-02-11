using Scrap.Jobs;

namespace Scrap;

public interface IJobFactory
{
    Task<Job> CreateAsync(NewJobDto newJobDto);
}
