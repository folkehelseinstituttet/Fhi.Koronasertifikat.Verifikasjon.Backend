using FHICORC.Application.Models;
using FHICORC.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FHICORC.ApplicationHost.Api.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [ApiVersion("1")]
    [ApiVersion("2")]
    [Route("v{version:apiVersion}/[controller]")]
    public class RevocationController : ControllerBase
    {
        private readonly IRevocationFetchService _revocationFetchService;
        private readonly IRevocationUploadService _revocationUploadService;

        public RevocationController(IRevocationFetchService revocationService)
        {
            _revocationFetchService = revocationService;
        }

        [HttpGet("certificate")]
        public IActionResult CheckCertificateRevocated([FromHeader] string dcc, [FromHeader] string country)
        {
            return Ok(_revocationFetchService.ContainsCertificate(dcc, country));
        }

        [HttpGet("download")]
        public IActionResult DownloadRevocationSuperBatches([FromHeader] DateTime lastDownloaded) {
            var superBatch = _revocationFetchService.FetchSuperBatches(lastDownloaded);

            if (superBatch is null || !superBatch.Any()) {
                return NoContent(); 
            }

            return Ok(superBatch);
        }

        [HttpGet("bucketinfo")]
        public IActionResult BucketInfo()
        {
            return Ok(_revocationFetchService.FetchBucketInfo());
        }

        [HttpPost("upload")]
        public IActionResult SendRevocationHashes([FromBody] IEnumerable<string> hashList)
        {
            return Ok(_revocationUploadService.UploadHashes(hashList));
        }

    }
}
