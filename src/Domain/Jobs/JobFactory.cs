namespace Scrap.Domain.Jobs;

public class JobFactory : IJobFactory
{
    private readonly IResourceRepositoryConfigurationValidatorFactory _validatorFactory;

    public JobFactory(
        IResourceRepositoryConfigurationValidatorFactory validatorFactory)
    {
        _validatorFactory = validatorFactory;
    }

    public async Task<Job> BuildAsync(JobDto jobDto)
    {
        var job = new Job(jobDto);
        if (job.ResourceRepoArgs != null)
        {
            var validator = await _validatorFactory.BuildAsync(job.ResourceRepoArgs);
            await validator.ValidateAsync(job.ResourceRepoArgs);
        }

        return job;
    }
}
