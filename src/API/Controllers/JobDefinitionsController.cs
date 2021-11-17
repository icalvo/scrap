using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Scrap.JobDefinitions;
using static Scrap.API.Controllers.ControllerResults;

namespace Scrap.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JobDefinitionsController : ControllerBase
    {
        private readonly JobDefinitionsApplicationService _applicationService;

        public JobDefinitionsController(JobDefinitionsApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<JobDefinitionDto>), 200)]
        public Task<ImmutableArray<JobDefinitionDto>> Search()
        {
            return _applicationService.GetJobsAsync();
        }

        [HttpGet]
        [Route("{nameOrId}")]
        [ProducesResponseType(typeof(JobDefinitionDto), 200)]
        public async Task<IActionResult> Get(string nameOrId)
        {
            if (Guid.TryParse(nameOrId, out var id))
            {
                return OkOrNotFound(await _applicationService.GetJobAsync(id));
            }

            var name = Uri.UnescapeDataString(nameOrId);
            return OkOrNotFound(await _applicationService.FindJobByNameAsync(name));
        }

        [HttpGet]
        [Route("ByUrl/{url}")]
        [ProducesResponseType(typeof(JobDefinitionDto), 200)]
        public async Task<IActionResult> GetByUrl(string url)
        {
            url = Uri.UnescapeDataString(url);
            return OkOrNotFound(await _applicationService.FindJobByRootUrlAsync(url));
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> Post([FromBody]NewJobDefinitionDto definition)
        {
            return Ok(await _applicationService.AddJobAsync(definition));
        }

        [HttpPut]
        [Route("{nameOrId}")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> Put(string nameOrId, [FromBody]NewJobDefinitionDto definition)
        {
            if (!Guid.TryParse(nameOrId, out var id))
            {
                var name = Uri.UnescapeDataString(nameOrId);
                return Ok(await _applicationService.UpsertAsync(name, definition));
            }
            
            return Ok(await _applicationService.UpsertAsync(id, definition));
        }        

        [HttpDelete]
        [Route("{nameOrId}")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> Delete(string nameOrId)
        {
            if (!Guid.TryParse(nameOrId, out var id))
            {
                var name = Uri.UnescapeDataString(nameOrId);
                var jobDef = await _applicationService.FindJobByNameAsync(name);
                if (jobDef == null)
                {
                    return NotFound();
                }

                id = jobDef.Id;
            }

            await _applicationService.DeleteJobAsync(id);
            return Ok(id.ToString());
        }
    }
}