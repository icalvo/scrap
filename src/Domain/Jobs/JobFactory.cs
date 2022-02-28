using Scrap.Domain.Resources;

namespace Scrap.Domain.Jobs;

public class JobFactory : IJobFactory
{
    private readonly IEntityRegistry<Job> _jobRegistry;
    private readonly Dictionary<string, IResourceRepositoryConfigurationValidator> _validators;

    public JobFactory(
        IEntityRegistry<Job> jobRegistry,
        IEnumerable<IResourceRepositoryConfigurationValidator> validators)
    {
        _jobRegistry = jobRegistry;
        _validators = validators.ToDictionary(x => x.RepositoryType);
    }

    public async Task<Job> CreateAsync(JobDto jobDto)
    {
        var job = new Job(jobDto);
        _jobRegistry.Register(job);
        await _validators[job.ResourceRepoArgs.RepositoryType].ValidateAsync(job.ResourceRepoArgs);
        return job;
    }
}
