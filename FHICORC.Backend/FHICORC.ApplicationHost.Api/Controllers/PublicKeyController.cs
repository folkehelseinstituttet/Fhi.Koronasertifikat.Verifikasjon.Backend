using Microsoft.AspNetCore.Mvc;
using FHICORC.Application.Services.Interfaces;
using System.Threading.Tasks;
using System;

namespace FHICORC.ApplicationHost.Api.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [ApiVersion("2")]
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/[controller]")]
    [Route("[controller]")]
    public class PublicKeyController : ControllerBase
    {
        private readonly IPublicKeyService _publicKeyService;
        public PublicKeyController(IPublicKeyService publicKeyService)
        {
            _publicKeyService = publicKeyService;
        }

        [HttpGet]
        [MapToApiVersion("1")]
        [Obsolete("Deprecated")]
        public IActionResult GetPublicKeyV1()
        {
            return StatusCode(410);
        }

        [HttpGet]
        [MapToApiVersion("2")]
        [Obsolete("Deprecated")]
        public IActionResult GetPublicKeyV2()
        {
            return StatusCode(410);
        }

        [HttpGet]
        [MapToApiVersion("3")]
        public async Task<IActionResult> GetPublicKeyV3()
        {
            var publicKeyResponseDto = await _publicKeyService.GetPublicKeysAsync();

            return Ok(publicKeyResponseDto.pkList);
        }
    }
}
