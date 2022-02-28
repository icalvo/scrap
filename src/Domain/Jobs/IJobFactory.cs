namespace Scrap.Domain.Jobs;

public interface IJobFactory
{
    Task<Job> CreateAsync(JobDto jobDto);
}
