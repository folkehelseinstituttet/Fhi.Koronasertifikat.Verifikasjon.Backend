﻿using Microsoft.AspNetCore.Mvc;
using FHICORC.Application.Services.Interfaces;
using System.Threading.Tasks;

namespace FHICORC.ApplicationHost.Api.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [ApiVersion("2")]
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
        public async Task<IActionResult> GetPublicKey()
        {
            var publicKeyResponseDto = await _publicKeyService.GetPublicKeysAsync();

            return Ok(publicKeyResponseDto.pkList);
        }

        [HttpGet]
        [MapToApiVersion("2")]
        public async Task<IActionResult> GetPublicKeyV2()
        {
            var publicKeyResponseDto = await _publicKeyService.GetPublicKeysAsync();

            return Ok(publicKeyResponseDto.pkList);
        }
    }
}