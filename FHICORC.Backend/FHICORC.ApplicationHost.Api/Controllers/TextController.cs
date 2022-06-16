using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FHICORC.Application.Models;
using FHICORC.Application.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace FHICORC.ApplicationHost.Api.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [ApiVersion("2")]
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/[controller]")]
    [Route("[controller]")]
    public class TextController : ControllerBase
    {
        private readonly ITextService _textService;
        private readonly ILogger<TextController> _logger;

        public TextController(ITextService textService, ILogger<TextController> logger)
        {
            _textService = textService;
            _logger = logger;
        }

        [HttpGet]
        [MapToApiVersion("1")]
        [Obsolete("Deprecated")]
        public IActionResult GetLatestVersionV1([FromHeader] TextRequestDto textRequestDto)
        {
            return StatusCode(410);
        }

        [HttpGet]
        [MapToApiVersion("2")]
        [Obsolete("Deprecated")]
        public IActionResult GetLatestVersionV2([FromHeader] TextRequestDto textRequestDto)
        {
            return StatusCode(410);
        }

        [HttpGet]
        [MapToApiVersion("3")]
        public async Task<IActionResult> GetLatestVersionV3([FromHeader] TextRequestDto textRequestDto)
        {
            return await GetTextVersionResponse(textRequestDto);
        }


        private async Task<IActionResult> GetTextVersionResponse(TextRequestDto textRequestDto)
        {
            try
            {
                _logger.LogDebug("Get Texts called with {VersionNo}", textRequestDto.CurrentVersionNo);

                var response = await _textService.GetLatestVersionAsync(textRequestDto);
                if (response.IsAppVersionUpToDate)
                {
                    return StatusCode(204);
                }
                else
                {
                    return new FileContentResult(response.ZipContents, "application/octet-stream");
                }
            }
            catch (Exception e)
            {
                return StatusCode(404, e.Message);
            }
        }
    }
}

