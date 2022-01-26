using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FHICORC.Application.Models.SmartHealthCard;
using FHICORC.Application.Services;
using FHICORC.Application.Services.Interfaces;
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
            string requestBody = string.Empty;
            ShcTrustRequestDto shcRequestDeserialized;
            try
            {
                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                shcRequestDeserialized = JsonSerializer.Deserialize<ShcTrustRequestDto>(
                    requestBody,
                    _jsonSerializerOptions);

                if (shcRequestDeserialized == null)
                {
                    throw new NullReferenceException("Body values could not be serialized");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"No application statistics found in body or unable to parse data. {ex} [Deserialized request]: {requestBody}");
                return BadRequest("No application statistics found in body or unable to parse data");
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred while trying get trusted: {ex}";
                _logger.LogError(errorMessage);
                return StatusCode(500);
            }

            var ret = _trustedIssuerService.GetIssuer(shcRequestDeserialized.iss);
            ShcTrustResponseDto dto = new ShcTrustResponseDto()
            {
                Trusted = ret != null,
                Name = ret?.Name
            };
            return Ok(dto);
        }
        
        [HttpPost]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("diag/AddIssuer")]
        public async Task<IActionResult> AddIssuer()
        {
            var requestBody = string.Empty;
            AddIssuersRequest shcRequestDeserialized;
            try
            {
                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                shcRequestDeserialized = JsonSerializer.Deserialize<AddIssuersRequest>(
                    requestBody,
                    _jsonSerializerOptions);

                if (shcRequestDeserialized == null)
                {
                    throw new NullReferenceException("Body values could not be serialized");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"No data found in body or unable to parse data. {ex} [Deserialized request]: {requestBody}");
                return BadRequest("No data found in body or unable to parse data");
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred while trying add trusted: {ex}";
                _logger.LogError(errorMessage);
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

        [HttpDelete]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("diag/CleanTableTrustedIssuer")]
        public async Task<IActionResult> CleanTableTrustedIssuer()
        {
            await _trustedIssuerService.RemoveAllIssuers();
            return Ok();
        }

        [HttpPut]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("diag/MarkAsUntrust")]
        public async Task<IActionResult> MarkAsUntrusted()
        {
            var requestBody = string.Empty;
            ShcTrustRequestDto shcRequestDeserialized;
            try
            {
                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                shcRequestDeserialized = JsonSerializer.Deserialize<ShcTrustRequestDto>(
                    requestBody,
                    _jsonSerializerOptions);

                if (shcRequestDeserialized == null)
                {
                    throw new NullReferenceException("Body values could not be serialized");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"No application statistics found in body or unable to parse data. {ex} [Deserialized request]: {requestBody}");
                return BadRequest("No application statistics found in body or unable to parse data");
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred while trying get trusted: {ex}";
                _logger.LogError(errorMessage);
                return StatusCode(500);
            }

            var ret = await _trustedIssuerService.MarkAsUntrusted(shcRequestDeserialized.iss);
            if (ret == false)
                return NotFound("Issuer not found.");
            return Ok(requestBody + " marked.");
        }

        [HttpDelete]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("diag/removeIssuer")]
        public async Task<IActionResult> RemoveIssuer()
        {
            var requestBody = string.Empty;
            ShcTrustRequestDto shcRequestDeserialized;
            try
            {
                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                shcRequestDeserialized = JsonSerializer.Deserialize<ShcTrustRequestDto>(
                    requestBody,
                    _jsonSerializerOptions);

                if (shcRequestDeserialized == null)
                {
                    throw new NullReferenceException("Body values could not be serialized");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"No application statistics found in body or unable to parse data. {ex} [Deserialized request]: {requestBody}");
                return BadRequest("No application statistics found in body or unable to parse data");
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred while trying get trusted: {ex}";
                _logger.LogError(errorMessage);
                return StatusCode(500);
            }

            var ret = await _trustedIssuerService.RemoveIssuer(shcRequestDeserialized.iss);
            if (ret == false)
                return NotFound("Issuer not found.");
            return Ok(requestBody + " removed.");
        }
        #endregion

        #region Vaccine Codes
        [HttpPost]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("vaccineinfo")]
        public async Task<IActionResult> GetVaccineInfo()
        {
            var requestBody = string.Empty;
            ShcCodeRequestDto shcRequestDeserialized;
            try
            {
                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                shcRequestDeserialized = JsonSerializer.Deserialize<ShcCodeRequestDto>(
                    requestBody,
                    _jsonSerializerOptions);

                if (shcRequestDeserialized == null)
                {
                    throw new NullReferenceException("Body values could not be serialized");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"No vaccine info found in body or unable to parse data. {ex} [Deserialized request]: {requestBody}");
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
        [Route("diag/AddVaccineCodes")]
        public async Task<IActionResult> AddVaccineCode()
        {
            var requestBody = string.Empty;
            VaccineCodesDto shcRequestDeserialized;
            try
            {
                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                shcRequestDeserialized = JsonSerializer.Deserialize<VaccineCodesDto>(
                    requestBody,
                    _jsonSerializerOptions);

                if (shcRequestDeserialized == null)
                {
                    throw new NullReferenceException("Body values could not be serialized");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"No data found in body or unable to parse data. {ex} [Deserialized request]: {requestBody}");
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
        [Route("diag/CleanTableVaccineCodes")]
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
                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                shcRequestDeserialized = JsonSerializer.Deserialize<VaccineCodeDto>(
                    requestBody,
                    _jsonSerializerOptions);

                if (shcRequestDeserialized == null)
                {
                    throw new NullReferenceException("Body values could not be serialized");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"No vaccine codes found in body or unable to parse data. {ex} [Deserialized request]: {requestBody}");
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

        [HttpGet]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("diag/printDir")]
        public IActionResult DiagnosticPrintDir()
        {
            return Ok(TrustedIssuerService.PrintFolder("."));
        }
    }
}