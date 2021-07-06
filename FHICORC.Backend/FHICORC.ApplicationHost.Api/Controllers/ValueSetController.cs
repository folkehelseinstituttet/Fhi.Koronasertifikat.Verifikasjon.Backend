using System;
using System.Threading.Tasks;
using FHICORC.Application.Models;
using FHICORC.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FHICORC.ApplicationHost.Api.Controllers
{
    [ApiController]
    [ApiVersion("2")]
    [Route("v{version:apiVersion}/[controller]")]
    [Route("[controller]")]
    public class ValueSetController : ControllerBase
    {
        private readonly IValueSetService _valueSetService;

        public ValueSetController(IValueSetService valueSetService)
        {
            _valueSetService = valueSetService;
        }

        [HttpGet]
        [MapToApiVersion("2")]
        public async Task<IActionResult> GetLatestVersion([FromHeader] ValueSetRequestDto valueSetRequestDto)
        {
            try
            {
                var response = await _valueSetService.GetLatestVersionAsync(valueSetRequestDto);
                if (response.IsAppVersionUpToDate)
                {
                    return StatusCode(204);
                }
                else
                {
                    return new FileContentResult(response.ZipContents, "application/octet-stream")
                        {LastModified = response.LastUpdated};
                }
            }
            catch (Exception e)
            {
                return StatusCode(404, e.Message);
            }
        }
    }
}
