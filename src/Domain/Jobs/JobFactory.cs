﻿using Scrap.Domain.Jobs;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.Domain;

public class JobFactory : IJobFactory
{
    private readonly IEntityRegistry<Job> _jobRegistry;
    private readonly IResourceRepositoryConfigurationValidator _validator;

    public JobFactory(
        IEntityRegistry<Job> jobRegistry,
        IResourceRepositoryConfigurationValidator validator)
    {
        _jobRegistry = jobRegistry;
        _validator = validator;
    }

    public async Task<Job> CreateAsync(JobDto jobDto)
    {
        var job = new Job(jobDto);
        await _validator.ValidateAsync(job.ResourceRepoArgs);
        _jobRegistry.Register(job);
        return job;
    }
}
