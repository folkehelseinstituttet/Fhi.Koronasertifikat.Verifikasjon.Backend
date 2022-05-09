﻿using FHICORC.Application.Models;
using FHICORC.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace FHICORC.ApplicationHost.Api.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [ApiVersion("1")]
    [Route("v{version:apiVersion}/[controller]")]
    public class RevocationController : ControllerBase
    {
        private readonly IRevocationService _revocationService;

        public RevocationController(IRevocationService revocationService)
        {
            _revocationService = revocationService;
        }


        [HttpGet("certificate")]
        public IActionResult CheckCertificateRevocated([FromHeader] string dcc)
        {
            return Ok(_revocationService.ContainsCertificate(dcc));
        }


        [HttpGet("download")]
        public SuperBatchesDto DownloadRevocationSuperBatches([FromHeader] DateTime lastDownloaded) {
            return _revocationService.FetchSuperBatches(lastDownloaded);
        }

        [HttpGet("bucketinfo")]
        public BloomFilterBuckets BucketInfo()
        {
            return _revocationService.FetchBucketInfo();
        }

    }
}
