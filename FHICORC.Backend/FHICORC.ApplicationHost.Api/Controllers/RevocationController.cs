using FHICORC.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace FHICORC.ApplicationHost.Api.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [ApiVersion("1")]
    [ApiVersion("2")]
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/[controller]")]
    public class RevocationController : ControllerBase
    {
        private readonly IRevocationFetchService _revocationFetchService;

        public RevocationController(IRevocationFetchService revocationService)
        {
            _revocationFetchService = revocationService;
        }

        [HttpGet("download")]
        public IActionResult DownloadRevocationSuperBatches([FromHeader] DateTime lastDownloaded) {
            var superBatch = _revocationFetchService.FetchSuperBatches(lastDownloaded);

            if (superBatch is null || !superBatch.Any()) {
                return NoContent(); 
            }

            return Ok(superBatch);
        }
    }
}
