using Hangfire;
using Scrap.DependencyInjection;
using Scrap.JobDefinitions;

namespace Scrap.CommandLine;

class CustomJobActivator : JobActivator
{
    private readonly ServicesResolver _serviceResolver;
    private static JobApplicationService? _jobApplicationService;
    private static JobDefinitionsApplicationService? _jobDefinitionsApplicationService;

    public CustomJobActivator(ServicesResolver serviceResolver)
    {
        _serviceResolver = serviceResolver;
    }

    public override object ActivateJob(Type jobType)
    {
        if (jobType == typeof(JobApplicationService))
        {
            return _jobApplicationService ??= _serviceResolver.BuildScrapperApplicationService();
        }
        
        if (jobType == typeof(JobDefinitionsApplicationService))
        {
            return _jobDefinitionsApplicationService ??= _serviceResolver.BuildJobDefinitionsApplicationServiceAsync().Result;
        }

        return base.ActivateJob(jobType);
    }
}
