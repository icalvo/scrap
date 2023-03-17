using Scrap.Domain.Resources;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.Domain.Jobs;

public class JobFactory : IJobFactory
{
    private readonly IResourceRepositoryConfigurationValidator _validator;

    public JobFactory(
        IResourceRepositoryConfigurationValidator validator)
    {
        _validator = validator;
    }

    public async Task<Job> BuildAsync(JobDto jobDto)
    {
        var job = new Job(jobDto);
        if (job.ResourceRepoArgs != null)
        {
            await _validator.ValidateAsync(job.ResourceRepoArgs);
        }

        return job;
    }
}
