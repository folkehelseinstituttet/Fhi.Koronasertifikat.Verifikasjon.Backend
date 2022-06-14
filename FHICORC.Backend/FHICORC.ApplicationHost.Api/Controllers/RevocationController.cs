using FHICORC.Application.Models;
using FHICORC.Application.Services;
using FHICORC.Infrastructure.Database.Context;
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
        private readonly CoronapassContext _coronapassContext;

        public RevocationController(IRevocationFetchService revocationService, CoronapassContext coronapassContext)
        {
            _revocationFetchService = revocationService;
            _coronapassContext = coronapassContext;
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


        [HttpGet("hashes")]
        public IActionResult DownloadHashes()
        {
            var superBatch = _coronapassContext.RevocationHash.Select(x => x.Hash).ToList();

            return Ok(superBatch);
        }
    }
}
