using System.Threading.Tasks;
using FHICORC.Application.Common.Interfaces;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FHICORC.Application.Services
{
    public class RuleService : IRuleService
    {
        private const string CacheKey = "ruleCacheKey";

        private readonly ICacheManager _cacheManager;
        private readonly RuleCacheOptions _ruleCacheOptions;
        private readonly ILogger<RuleService> _logger;
        private readonly IMetricLogService _metricLogService;
        private readonly IBusinessRuleRepository _businessRuleRepository;

        public RuleService(ICacheManager cacheManager, RuleCacheOptions ruleCacheOptions,ILogger<RuleService> logger,
            IMetricLogService metricLogService, IBusinessRuleRepository businessRuleRepository)
        {
            _cacheManager = cacheManager;
            _ruleCacheOptions = ruleCacheOptions;
            _logger = logger;
            _metricLogService = metricLogService;
            _businessRuleRepository = businessRuleRepository;
        }

        public async Task<RuleResponseDto> GetRulesAsync()
        {
            if (_cacheManager.TryGetValue(CacheKey, out RuleResponseDto cachedData))
            {
                _logger.LogDebug("Rules found in cache");
                _metricLogService.AddMetric("Rule_CacheHit", true);
                return cachedData;
            }
            _logger.LogDebug("Rules not found in cache. Fetching from repository");
            _metricLogService.AddMetric("Rule_CacheHit", false);

            var databaseResults = await _businessRuleRepository.GetAllBusinessRules();

            var ruleResponseDto = new RuleResponseDto {RuleListJson = databaseResults.ToString()};
            _cacheManager.Set(CacheKey, ruleResponseDto, _ruleCacheOptions);

            return ruleResponseDto;
        }
    }
}