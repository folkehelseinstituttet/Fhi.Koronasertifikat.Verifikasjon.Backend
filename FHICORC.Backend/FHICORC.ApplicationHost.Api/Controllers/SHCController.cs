using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FHICORC.Application.Models;
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
        private readonly ISHCService _shcService;
        private readonly ILogger<ShCController> _logger;
        private readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            IgnoreNullValues = false
        };

        public ShCController(ISHCService shcService, ILogger<ShCController> logger)
        {
            _shcService = shcService;
            _logger = logger;
        }

        [HttpPost]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("trust")]
        public async Task<IActionResult> GetIsTrusted()
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
                var errorMessage = $"An error occurred while trying to save application statistics: {ex}";
                _logger.LogError(errorMessage);
                return StatusCode(500);
            }

            var trustResponseDto = await _shcService.GetIsTrustedsync(shcRequestDeserialized);
            return Ok(trustResponseDto);
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

            var vaccineResponseDto = await _shcService.GetVaccinationInfosync(shcRequestDeserialized);
            return Ok(vaccineResponseDto);
        }
    }
}