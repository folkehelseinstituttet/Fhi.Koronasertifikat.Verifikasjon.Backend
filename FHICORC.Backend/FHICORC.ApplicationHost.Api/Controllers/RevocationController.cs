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
    [Route("v{version:apiVersion}/[controller]")]
    public class RevocationController : ControllerBase
    {

        private readonly IRevocationService _bloomFilterService;

        public RevocationController(IRevocationService bloomFilterService)
        {
            _bloomFilterService = bloomFilterService;
        }


        [HttpGet("certificate")]
        public IActionResult CheckCertificateRevocated([FromHeader] string dcc)
        {

            return Ok(_bloomFilterService.ContainsCertificate(dcc));
        }

        //[HttpGet("offlinefilter")]
        //public IActionResult GetOfflineRevocationList() {

        //    return Ok(_bloomFilterService.GetFilterRevocList());
        //}

        //[HttpPost("addrevocatedcertificate")]
        //public IActionResult AddRevocatedCertificate(string dcc)
        //{
        //    _bloomFilterService.AddToRevocation(dcc);
        //    return Ok();
        //}


        //[HttpGet("manual")]
        //public IActionResult ManualActivateFilterRestructure() {

        //    _bloomFilterService.CreateSuperFilter();
        //    return Ok();

        //}


    }
}
