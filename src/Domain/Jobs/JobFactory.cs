using Scrap.Domain.Resources;

namespace Scrap.Domain.Jobs;

public class JobFactory : IAsyncFactory<JobDto, Job>
{
    private readonly IFactory<IResourceRepositoryConfiguration, IResourceRepositoryConfigurationValidator> _validatorFactory;

    public JobFactory(IFactory<IResourceRepositoryConfiguration, IResourceRepositoryConfigurationValidator> validatorFactory)
    {
        _validatorFactory = validatorFactory;
    }

    public async Task<Job> Build(JobDto jobDto)
    {
        var job = new Job(jobDto);
        if (job.ResourceRepoArgs != null)
        {
            var validator = _validatorFactory.Build(job.ResourceRepoArgs);
            await validator.ValidateAsync(job.ResourceRepoArgs);
        }

        return job;
    }
}
