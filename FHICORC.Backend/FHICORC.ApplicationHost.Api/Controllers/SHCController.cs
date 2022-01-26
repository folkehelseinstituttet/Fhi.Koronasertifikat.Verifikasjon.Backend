using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FHICORC.Application.Models.SmartHealthCard;
using FHICORC.Application.Services;
using FHICORC.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace FHICORC.ApplicationHost.Api.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [ApiVersion("2")]
    [Route("v{version:apiVersion}/[controller]")]
    [Route("[controller]")]
    public class SHCController : ControllerBase
    {
        private readonly ITrustedIssuerService _trustedIssuerService;
        private readonly IVaccineCodesService _vaccineCodesService;
        private readonly ILogger<SHCController> _logger;

        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            IgnoreNullValues = false
        };

        public SHCController(
            ITrustedIssuerService trustedIssuerService,
            IVaccineCodesService vaccineCodesService,
            ILogger<SHCController> logger)
        {
            _trustedIssuerService = trustedIssuerService;
            _vaccineCodesService = vaccineCodesService;
            _logger = logger;
        }

        #region TrustedIssuer
        [HttpPost]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("trust")]
        public async Task<IActionResult> GetIsTrusted()
        {
            ShcTrustRequestDto shcRequestDeserialized;
            try
            {
                shcRequestDeserialized = await ParseHttpBody<ShcTrustRequestDto>();
            }
            catch (JsonException)
            {
                return BadRequest("No application statistics found in body or unable to parse data");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while trying get trusted: {ex}");
                return StatusCode(500);
            }

            var issuer = _trustedIssuerService.GetIssuer(shcRequestDeserialized.Iss);
            ShcTrustResponseDto dto = new ShcTrustResponseDto()
            {
                Trusted = issuer?.IsTrusted ?? false,
                Name = issuer?.Name
            };
            return Ok(dto);
        }
        
        [HttpPost]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("diag/addIssuer")]
        public async Task<IActionResult> AddIssuer()
        {
            AddIssuersRequest shcRequestDeserialized;
            try
            {
                shcRequestDeserialized = await ParseHttpBody<AddIssuersRequest>();
            }
            catch (JsonException)
            {
                return BadRequest("No data found in body or unable to parse data");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while trying add trusted: {ex}");
                return StatusCode((int) HttpStatusCode.InternalServerError);
            }

            try
            {
                await _trustedIssuerService.AddIssuers(shcRequestDeserialized, true);
            }
            catch (Exception e)
            {
                return StatusCode((int) HttpStatusCode.InternalServerError,
                    "Error: " + e.Message + e.InnerException + e.StackTrace);
            }
            return StatusCode((int)HttpStatusCode.Created);
        }

        [HttpPut]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("diag/trustIssuer")]
        public async Task<IActionResult> TrustIssuer()
        {
            return await UpdateIsTrusted(true);
        }

        [HttpPut]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("diag/distrustIssuer")]
        public async Task<IActionResult> DistrustIssuer()
        {
            return await UpdateIsTrusted(false);
        }

        private async Task<IActionResult> UpdateIsTrusted(bool trusted)
        {
            ShcTrustRequestDto shcRequestDeserialized;
            try
            {
                shcRequestDeserialized = await ParseHttpBody<ShcTrustRequestDto>();
            }
            catch (JsonException)
            {
                return BadRequest("No application statistics found in body or unable to parse data");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while trying to uppdate trusted: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            bool success = _trustedIssuerService.UpdateIsTrusted(shcRequestDeserialized.Iss, trusted);
            if (success)
            {
                return Ok();
            } else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpDelete]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("diag/cleanTableTrustedIssuer")]
        public async Task<IActionResult> CleanTableTrustedIssuer()
        {
            await _trustedIssuerService.RemoveAllIssuers();
            return Ok();
        }

        [HttpDelete]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("diag/removeIssuer")]
        public async Task<IActionResult> RemoveIssuer()
        {
            ShcTrustRequestDto shcRequestDeserialized;
            try
            {
                shcRequestDeserialized = await ParseHttpBody<ShcTrustRequestDto>();
            }
            catch (JsonException)
            {
                return BadRequest("No application statistics found in body or unable to parse data");
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred while trying get trusted: {ex}";
                _logger.LogError(errorMessage);
                return StatusCode(500);
            }

            var ret = await _trustedIssuerService.RemoveIssuer(shcRequestDeserialized.Iss);
            if (ret == false)
                return NotFound("Issuer not found.");
            return Ok(shcRequestDeserialized.Iss + " removed.");
        }
        #endregion

        #region Vaccine Codes
        [HttpPost]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("vaccineinfo")]
        public async Task<IActionResult> GetVaccineInfo()
        {
            ShcCodeRequestDto shcRequestDeserialized;
            try
            {
                shcRequestDeserialized = await ParseHttpBody<ShcCodeRequestDto>();
            }
            catch (JsonException)
            {
                return BadRequest("No vaccine info found in body or unable to parse data");
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred while trying to save application statistics: {ex}";
                _logger.LogError(errorMessage);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            var vaccineResponseDto = await _vaccineCodesService.GetVaccinationInfo(shcRequestDeserialized);
            if (vaccineResponseDto == null)
                return NotFound("Code not found.");
            return Ok(vaccineResponseDto);
        }

        [HttpPost]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("diag/addVaccineCodes")]
        public async Task<IActionResult> AddVaccineCode()
        {
            VaccineCodesDto shcRequestDeserialized;
            try
            {
                shcRequestDeserialized = await ParseHttpBody<VaccineCodesDto>();
            }
            catch (JsonException)
            {
                return BadRequest("No data found in body or unable to parse data");
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred while trying add trusted: {ex}";
                _logger.LogError(errorMessage);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            try
            {
                await _vaccineCodesService.AddVaccineCode(shcRequestDeserialized, true);
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Error: " + e.Message + e.InnerException + e.StackTrace);
            }
            return StatusCode((int) HttpStatusCode.Created, "Vaccine code added");
        }

        [HttpDelete]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("diag/cleanTableVaccineCodes")]
        public async Task<IActionResult> CleanTableVaccineCodes()
        {
            await _vaccineCodesService.RemoveAllVaccineCodes();
            return Ok();
        }

        [HttpDelete]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("diag/removeVaccineCode")]
        public async Task<IActionResult> RemoveVaccineCode()
        {
            var requestBody = string.Empty;
            VaccineCodeDto shcRequestDeserialized;
            try
            {
                shcRequestDeserialized = await ParseHttpBody<VaccineCodeDto>();
            }
            catch (JsonException)
            {
                return BadRequest("No vaccine codes found in body or unable to parse data");
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred while trying get trusted: {ex}";
                _logger.LogError(errorMessage);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            var ret = await _vaccineCodesService.RemoveVaccineCode(new VaccineCodeKey(){VaccineCode = shcRequestDeserialized.Code, CodingSystem = shcRequestDeserialized.System });
            if (ret == false)
                return NotFound("Code not found.");
            return Ok(requestBody + " removed.");
        }
        #endregion

        #region Other
        [HttpGet]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("diag/printDir")]
        public IActionResult DiagnosticPrintDir()
        {
            return Ok(TrustedIssuerService.PrintFolder("."));
        }

        private async Task<T> ParseHttpBody<T>()
        {
            string requestBody = string.Empty;
            try
            {
                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                T deserialized = JsonSerializer.Deserialize<T>(
                    requestBody,
                    _jsonSerializerOptions);

                if (deserialized == null)
                {
                    throw new NullReferenceException("Body values could not be deserialized");
                }
                return deserialized;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"No application statistics found in body or unable to parse data. {ex} [Deserialized request]: {requestBody}");
                throw;
            }
        }
        #endregion
    }
}