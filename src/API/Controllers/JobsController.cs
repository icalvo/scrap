using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Scrap;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly ILogger<JobsController> _logger;
        private readonly ScrapperApplicationService _applicationService;

        public JobsController(ScrapperApplicationService applicationService, ILogger<JobsController> logger)
        {
            _logger = logger;
            _applicationService = applicationService;
        }

        [HttpPost]
        [Route("{name}")]
        public Task<string> Run(string name, [FromQuery]string? rootUrl, [FromQuery]bool? fullScan, [FromQuery]bool? whatIf)
        {
            return Task.FromResult(BackgroundJob.Enqueue(
                () => _applicationService.ScrapAsync(name, rootUrl, fullScan, whatIf)));
        }
    }
}