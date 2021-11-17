using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Scrap.Jobs;
using Scrap.Resources;

namespace Scrap.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly JobApplicationService _applicationService;
        private readonly JobDefinitionsApplicationService _definitionsApplicationService;

        public JobsController(JobApplicationService applicationService, JobDefinitionsApplicationService definitionsApplicationService)
        {
            _applicationService = applicationService;
            _definitionsApplicationService = definitionsApplicationService;
        }

        [HttpPost]
        [Route("{name}")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> Run(string name, JobOptionsWithRootUrl optionsOverride)
        {
            var (configuration, fullScan, whatIf, rootUrl) = optionsOverride;

            var jobDef = await _definitionsApplicationService.FindJobByNameAsync(name);
            if (jobDef == null)
            {
                return NotFound();
            }

            var newJob = new NewJobDto(jobDef, rootUrl, whatIf, fullScan, configuration);
            return Ok(BackgroundJob.Enqueue(() => _applicationService.RunAsync(newJob)));
        }

        public class JobOptionsBasic
        {
            public IResourceProcessorConfiguration? Configuration { get; }
            public bool? FullScan { get; }
            public bool? WhatIf { get; }

            public void Deconstruct(out IResourceProcessorConfiguration? configuration, out bool? fullScan, out bool? whatIf)
            {
                configuration = Configuration;
                fullScan = FullScan;
                whatIf = WhatIf;
            }
        }

        public class JobOptionsWithRootUrl : JobOptionsBasic
        {
            public string? RootUrl { get; }

            public void Deconstruct(out IResourceProcessorConfiguration? configuration, out bool? fullScan, out bool? whatIf, out string? rootUrl)
            {
                configuration = Configuration;
                fullScan = FullScan;
                whatIf = WhatIf;
                rootUrl = RootUrl;
            }
        }

        [HttpPost]
        [Route("ByUrl/{rootUrl}")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> RunFromUrl(string rootUrl, [FromBody]JobOptionsBasic optionsOverride)
        {
            var (configuration, fullScan, whatIf) = optionsOverride;
            var jobDef = await _definitionsApplicationService.FindJobByRootUrlAsync(rootUrl);
            if (jobDef == null)
            {
                return NotFound();
            }

            var newJob = new NewJobDto(jobDef, rootUrl, whatIf, fullScan, configuration);
            return Ok(BackgroundJob.Enqueue(() => _applicationService.RunAsync(newJob)));
        }
    }
}