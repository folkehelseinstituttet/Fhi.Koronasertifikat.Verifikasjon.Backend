using System;
using System.IO;
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
    public class ShCController : ControllerBase
    {
        private readonly ITrustedIssuerService _trustedIssuerService;
        private readonly ILogger<ShCController> _logger;

        private readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            IgnoreNullValues = false
        };

        public ShCController(ITrustedIssuerService trustedIssuerService, ILogger<ShCController> logger)
        {
            _trustedIssuerService = trustedIssuerService;
            _logger = logger;
        }

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
                    jsonSerializerOptions);

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
            ShcTrustResponseDto dto = new()
            {
                Trusted = ret != null,
                Name = ret?.Name
            };
            return Ok(dto);
        }

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
                    jsonSerializerOptions);

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
                var errorMessage = $"An error occurred while trying to save application statistics: {ex}";
                _logger.LogError(errorMessage);
                return StatusCode(500);
            }

            var vaccineResponseDto = await _trustedIssuerService.GetVaccinationInfosync(shcRequestDeserialized);
            return Ok(vaccineResponseDto);
            //return Content("TEST SHC", "application/json", Encoding.UTF8);
        }

        [HttpGet]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("diag/printDir")]
        public async Task<IActionResult> DiagnosticPrintDir()
        {
            return Ok(TrustedIssuerService.PrintFolder("."));
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
                    jsonSerializerOptions);

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
                return StatusCode(500);
            }

            await _trustedIssuerService.AddIssuers(shcRequestDeserialized, true);
            return NoContent();
        }

        [HttpDelete]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("diag/CleanTable")]
        public async Task<IActionResult> CleanTable()
        {
            await _trustedIssuerService.RemoveAllIssuers();
            return NoContent();
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
                    jsonSerializerOptions);

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
                return Ok("Issuer not found.");
            return Ok(requestBody + " removed.");
        }
    }
}