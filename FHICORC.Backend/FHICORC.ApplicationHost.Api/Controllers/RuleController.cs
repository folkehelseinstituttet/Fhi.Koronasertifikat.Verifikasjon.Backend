using System;
using System.Text;
using System.Threading.Tasks;
using FHICORC.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FHICORC.ApplicationHost.Api.Controllers
{
    [ApiController]
    [ApiVersion("2")]
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/[controller]")]
    [Route("[controller]")]
    public class RuleController : ControllerBase
    {
        private readonly IRuleService _ruleService;

        public RuleController(IRuleService ruleService)
        {
            _ruleService = ruleService;
        }

        [HttpGet]
        [MapToApiVersion("2")]
        [Obsolete("Deprecated")]
        public IActionResult GetRulesV2()
        {
            return StatusCode(410);
        }

        [HttpGet]
        [MapToApiVersion("3")]
        public async Task<IActionResult> GetRulesV3()
        {
            var ruleResponseDto = await _ruleService.GetRulesAsync();
            return Content(ruleResponseDto.RuleListJson, "application/json", Encoding.UTF8);
        }
    }
}