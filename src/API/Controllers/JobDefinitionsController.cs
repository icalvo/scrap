using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Scrap.JobDefinitions;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JobDefinitionsController : ControllerBase
    {
        private readonly ILogger<JobDefinitionsController> _logger;
        private readonly JobDefinitionsApplicationService _applicationService;

        public JobDefinitionsController(JobDefinitionsApplicationService applicationService, ILogger<JobDefinitionsController> logger)
        {
            _logger = logger;
            _applicationService = applicationService;
        }

        [HttpGet]
        public Task<ImmutableArray<JobDefinitionDto>> Search()
        {
            return _applicationService.GetJobsAsync();
        }

        [HttpGet]
        [Route("{name}")]
        public Task<JobDefinitionDto> Get(string name)
        {
            name = Uri.UnescapeDataString(name);
            return _applicationService.GetJobAsync(name);
        }
        
        [HttpPost]
        [Route("{name}")]
        public Task Post(string name, [FromBody]JobDefinitionDto definition)
        {
            name = Uri.UnescapeDataString(name);
            return _applicationService.AddJobAsync(name, definition);
        }        
        
        [HttpDelete]
        [Route("{name}")]
        public Task Delete(string name)
        {
            name = Uri.UnescapeDataString(name);
            return _applicationService.DeleteJobAsync(name);
        }
    }
}