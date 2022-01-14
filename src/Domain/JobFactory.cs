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

    public Job Create(NewJobDto newJobDto)
    {
        var job = new Job(newJobDto);
        job.ResourceRepoArgs.Validate(_loggerFactory);

        return job;
    }
}