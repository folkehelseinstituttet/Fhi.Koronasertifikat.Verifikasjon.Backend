using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FHICORC.Application.Models;
using FHICORC.Application.Services;
using FHICORC.Application.Services.Interfaces;
using FHICORC.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

        private readonly ITrustedIssuerService _shcService;
        private readonly ILogger<ShCController> _logger;

        public ShCController(ITrustedIssuerService trustedIssuerService, ILogger<ShCController> logger)
        {
            _shcService = trustedIssuerService;
            _logger = logger;
        }

        [HttpPost]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("trust")]
        public async Task<IActionResult> GetIsTrusted()
        {
            var requestBody = string.Empty;
            var shcRequestDeserialized = new ShcTrustRequestDto();
            try
            {
                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                shcRequestDeserialized = JsonSerializer.Deserialize<ShcTrustRequestDto>(
                    requestBody,
                    new JsonSerializerOptions { IgnoreNullValues = false });

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


            var trustResponseDto = await _shcService.GetIsTrustedsync(shcRequestDeserialized);
            return Ok(trustResponseDto);
            //return Content(trustResponseDto.Trusted.ToString(), "application/json", Encoding.UTF8);
        }

        [HttpPost]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("trustDB")]
        public async Task<IActionResult> GetIssuer()
        {
            var requestBody = string.Empty;
            var shcRequestDeserialized = new ShcTrustRequestDto();

            try
            {
                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                shcRequestDeserialized = JsonSerializer.Deserialize<ShcTrustRequestDto>(
                    requestBody,
                    new JsonSerializerOptions { IgnoreNullValues = false });

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

            var ret = await _shcService.GetIssuer(shcRequestDeserialized.iss);
            if (ret == null)
                return Ok("Issuer not found.");
            ShcTrustResponseDto dto = new ShcTrustResponseDto()
            {
                Trusted = true,
                Name = ret.Name
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
            var shcRequestDeserialized = new ShcCodeRequestDto();
            try
            {
                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                shcRequestDeserialized = JsonSerializer.Deserialize<ShcCodeRequestDto>(
                    requestBody,
                    new JsonSerializerOptions { IgnoreNullValues = false });

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

        [HttpGet]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("diag/AddIssuer")]
        public async Task<IActionResult> AddIssuer()
        {
            var requestBody = string.Empty;
            var shcRequestDeserialized = new AddIssuersRequest();

            try
            {
                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                shcRequestDeserialized = JsonSerializer.Deserialize<AddIssuersRequest>(
                    requestBody,
                    new JsonSerializerOptions { IgnoreNullValues = false });

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



            var ret = await _shcService.AddIssuer(shcRequestDeserialized);
            return Ok(ret);
        }

        [HttpGet]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("diag/CleanTable")]
        public async Task<IActionResult> CleanTable()
        {
            await _shcService.CleanTable();
            return Ok();
        }

        [HttpGet]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [Route("diag/removeIssuer")]
        public async Task<IActionResult> RemoveIssuer()
        {
            var requestBody = string.Empty;
            var shcRequestDeserialized = new ShcTrustRequestDto();

            try
            {
                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                shcRequestDeserialized = JsonSerializer.Deserialize<ShcTrustRequestDto>(
                    requestBody,
                    new JsonSerializerOptions { IgnoreNullValues = false });

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

            var ret = await _shcService.RemoveIssuer(shcRequestDeserialized.iss);
            if (ret == false)
                return Ok("Issuer not found.");
            return Ok(requestBody + " removed.");
        }
    }
}