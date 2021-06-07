using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using FHICORC.Application.Models;
using FHICORC.Application.Services.Interfaces;

namespace FHICORC.Application.Services
{
    public class PublicKeyServiceHealthCheck : IHealthCheck
    {
        private readonly IPublicKeyService _publicKeyService;

        public PublicKeyServiceHealthCheck(IPublicKeyService publicKeyService)
        {
            _publicKeyService = publicKeyService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                PublicKeyResponseDto publicKeyResponseDto = await _publicKeyService.GetPublicKeysAsync();

                if (!publicKeyResponseDto.pkList.Any())
                {
                    return HealthCheckResult.Unhealthy("Certificates cannot be fetched");
                }
            }
            catch (Exception)
            {
                return HealthCheckResult.Unhealthy("Certificates cannot be fetched");
            }

            return HealthCheckResult.Healthy();
        }
    }
}
