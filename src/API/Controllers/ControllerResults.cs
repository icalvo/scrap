using Microsoft.AspNetCore.Mvc;

namespace Scrap.API.Controllers
{
    public static class ControllerResults
    {
        public static IActionResult OkOrNotFound(object? value)
        {
            return value == null ? new NotFoundResult() : new OkObjectResult(value);
        }
    }
}