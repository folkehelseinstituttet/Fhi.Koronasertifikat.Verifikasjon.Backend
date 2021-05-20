using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;

namespace FHICORC.ApplicationHost.Api.Controllers
{
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        [Route("error")]
        public IActionResult HandleThrownExceptions()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context.Error;
            var errorCode = 500;
            var errorMessage = "Internal Server Error.";

            if (exception is BrokenCircuitException)
            {
                errorCode = 504;
                errorMessage = "Service Unavailable";
            }

            return StatusCode(errorCode, errorMessage);
        }

    }
}
