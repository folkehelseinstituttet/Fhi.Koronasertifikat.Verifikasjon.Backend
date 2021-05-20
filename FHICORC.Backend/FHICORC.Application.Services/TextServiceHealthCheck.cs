using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using FHICORC.Application.Models;
using FHICORC.Application.Services.Interfaces;

namespace FHICORC.Application.Services
{
    public class TextServiceHealthCheck : IHealthCheck
    {
        private readonly ITextService _textService;

        public TextServiceHealthCheck(ITextService textService)
        {
            _textService = textService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            TextRequestDto textRequestDto = new TextRequestDto
            {
                CurrentVersionNo = "0.0"
            };

            try
            {
                TextResponseDto textResponseDto = await _textService.GetLatestVersionAsync(textRequestDto);

                if (!textResponseDto.IsZipFileCreated)
                {
                    return HealthCheckResult.Unhealthy("Texts cannot be fetched");
                }
            }
            catch (Exception)
            {
                return HealthCheckResult.Unhealthy("Texts cannot be fetched");
            }

            return HealthCheckResult.Healthy();
        }
    }
}
