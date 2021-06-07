using Microsoft.AspNetCore.Mvc;
using FHICORC.Application.Services.Interfaces;
using System.Threading.Tasks;

namespace FHICORC.ApplicationHost.Api.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    // [ApiVersion("2")] - add me when relevant. Remember to add [MapToApiVersion("2")] to new version of existing endpoints.
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
        public async Task<IActionResult> GetPublicKey()
        {
            var publicKeyResponseDto = await _publicKeyService.GetPublicKeysAsync();

            return Ok(publicKeyResponseDto.pkList);
        }
    }
}
