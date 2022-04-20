using FHICORC.Application.Models.Revocation;
using FHICORC.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FHICORC.ApplicationHost.Api.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [ApiVersion("1")]
    [ApiVersion("2")]
    [Route("v{version:apiVersion}/[controller]")]
    [Route("[controller]")]
    public class RevocationController : ControllerBase
    {

        private readonly IBloomFilterService _bloomFilterService;

        public RevocationController(IBloomFilterService bloomFilterService)
        {
            _bloomFilterService = bloomFilterService;
        }

        [HttpGet("add")]
        public IActionResult AddToFilter()
        {
            _bloomFilterService.AddToFilterTest();

            return Ok("suppp");
        }

        [HttpGet("read")]
        public IActionResult ReadFilter()
        {
            _bloomFilterService.AddToFilterTest();
            return Ok("lol");
        }


        [HttpGet("certificate")]
        public IActionResult CheckCertificateRevocated([FromHeader] string dcc) {

            return Ok(_bloomFilterService.ContainsCertificate(dcc));
        }

        [HttpGet("offlinefilter")]
        public IActionResult GetOfflineRevocationList() {

            return Ok(_bloomFilterService.GetFilterRevocList());
        }

        [HttpPost("addrevocatedcertificate")]
        public IActionResult AddRevocatedCertificate(string dcc)
        {
            _bloomFilterService.AddToRevocation(dcc);
            return Ok();

        }


    }
}
