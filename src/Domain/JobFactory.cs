using Microsoft.Extensions.Logging;
using Scrap.Jobs;

namespace Scrap;

public class JobFactory : IJobFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public JobFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public async Task<Job> CreateAsync(NewJobDto newJobDto)
    {
        var job = new Job(newJobDto);
        await job.ResourceRepoArgs.ValidateAsync(_loggerFactory);

        return job;
    }
}
