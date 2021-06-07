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
    // [ApiVersion("2")] - add me when relevant. Remember to add [MapToApiVersion("2")] to new version of existing endpoints.
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
        public async Task<IActionResult> GetLatestVersion([FromHeader] TextRequestDto textRequestDto)
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

