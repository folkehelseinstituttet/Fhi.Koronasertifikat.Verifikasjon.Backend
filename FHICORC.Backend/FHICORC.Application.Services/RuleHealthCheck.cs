using System;
using System.Threading;
using System.Threading.Tasks;
using FHICORC.Application.Models;
using FHICORC.Application.Services.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FHICORC.Application.Services
{
    public class RuleHealthCheck : IHealthCheck
    {
        private readonly IRuleService _ruleService;

        public RuleHealthCheck(IRuleService ruleService)
        {
            _ruleService = ruleService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                RuleResponseDto ruleResponseDto = await _ruleService.GetRulesAsync();

                if (string.IsNullOrWhiteSpace(ruleResponseDto.RuleListJson))
                {
                    return HealthCheckResult.Unhealthy("Rules cannot be fetched");
                }
            }
            catch (Exception)
            {
                return HealthCheckResult.Unhealthy("Rules cannot be fetched");
            }

            return HealthCheckResult.Healthy();
        }
    }
}
