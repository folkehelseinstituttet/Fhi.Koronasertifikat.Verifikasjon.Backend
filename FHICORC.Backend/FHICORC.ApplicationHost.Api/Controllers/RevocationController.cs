using FHICORC.Application.Models;
using FHICORC.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace FHICORC.ApplicationHost.Api.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [ApiVersion("1")]
    [ApiVersion("2")]
    [Route("v{version:apiVersion}/[controller]")]
    public class RevocationController : ControllerBase
    {
        private readonly IRevocationService _revocationService;

        public RevocationController(IRevocationService revocationService)
        {
            _revocationService = revocationService;
        }


        [HttpGet("certificate")]
        public IActionResult CheckCertificateRevocated([FromHeader] string dcc, [FromHeader] string country)
        {
            return Ok(_revocationService.ContainsCertificate(dcc, country));
        }


        [HttpGet("download")]
        public IActionResult DownloadRevocationSuperBatches([FromHeader] DateTime lastDownloaded) {
            var superBatch = _revocationService.FetchSuperBatches(lastDownloaded);
            if (superBatch == null) {
                return NoContent(); 
            }

            superBatch = new List<SuperBatch>() { new SuperBatch() { BucketType = 424242} };
            return Ok(superBatch);
        }

        [HttpGet("bucketinfo")]
        public IActionResult BucketInfo()
        {
            return Ok(_revocationService.FetchBucketInfo());
        }

    }
}
