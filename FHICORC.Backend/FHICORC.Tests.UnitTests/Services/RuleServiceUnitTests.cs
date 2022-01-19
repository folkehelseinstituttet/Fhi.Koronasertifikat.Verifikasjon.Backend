using System;
using System.Threading.Tasks;
using FHICORC.Application.Common;
using FHICORC.Application.Common.Interfaces;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace FHICORC.Tests.UnitTests.Services
{
    [Category("Unit")]
    public class RuleServiceUnitTests
    {
        private ILogger<RuleService> _nullLogger = new NullLoggerFactory().CreateLogger<RuleService>();
        private readonly Mock<IMetricLogService> _metricLogServiceMock = new();
        private readonly Mock<RuleCacheOptions> _ruleCacheOptionsMock = new();
        private ICacheManager _cacheManager;
        private readonly Mock<IBusinessRuleRepository> _businessRuleRepositoryMock = new();

        private RuleService _ruleService;
        
        private const string JsonString = "[{ \"rule\": \"exampleRule\" }]";
        private string JTokenToString = "[" + Environment.NewLine + "  {" + Environment.NewLine +
            "    \"rule\": \"exampleRule\"" + Environment.NewLine + "  }" + Environment.NewLine + "]";
        private readonly RuleResponseDto _ruleResponseDto;
        private readonly JToken _businessRules = JToken.Parse(JsonString);

        public RuleServiceUnitTests()
        {
            _ruleResponseDto = new() { RuleListJson = JTokenToString };
        }

        [SetUp]
        public void SetUp()
        {
            _ruleCacheOptionsMock.Object.AbsoluteExpiration = 1440;
            _ruleCacheOptionsMock.Object.SlidingExpiration = 1440;
            _ruleCacheOptionsMock.Object.CacheSize = 1024;

            var services = new ServiceCollection();
            services.AddMemoryCache();
            services.AddSingleton<ICacheManager, CacheManager>();
            var serviceProvider = services.BuildServiceProvider();
            _cacheManager = serviceProvider.GetService<ICacheManager>();
            
            _ruleService = new RuleService(_cacheManager, _ruleCacheOptionsMock.Object, _nullLogger, 
                _metricLogServiceMock.Object, _businessRuleRepositoryMock.Object);
            
            _businessRuleRepositoryMock.Reset();
        }
        
        [Test]
        public async Task GetRulesAsync_When_CacheIsPopulated_WillReturnDataFromCache()
        {
            _cacheManager.Set("ruleCacheKey", _ruleResponseDto, _ruleCacheOptionsMock.Object);

            var result = await _ruleService.GetRulesAsync();

            _businessRuleRepositoryMock.Verify(x => x.GetAllBusinessRules(), Times.Never);
            Assert.AreEqual(_ruleResponseDto, result);
        }

        [Test]
        public async Task GetRulesAsync_When_CacheIsEmpty_Will_PopulateCache()
        {
            _businessRuleRepositoryMock.Setup(x => x.GetAllBusinessRules()).ReturnsAsync(_businessRules);
            
            Assert.IsFalse(_cacheManager.TryGetValue("ruleCacheKey", out RuleResponseDto emptyResponse));
            
            await _ruleService.GetRulesAsync();
            
            Assert.IsTrue(_cacheManager.TryGetValue("ruleCacheKey", out RuleResponseDto cachedData));
            Assert.AreEqual(JTokenToString, cachedData.RuleListJson);
        }

        [Test]
        public async Task GetRulesAsync_When_CacheIsEmpty_Will_FetchRulesFromRepository()
        {
            _businessRuleRepositoryMock.Setup(x => x.GetAllBusinessRules()).ReturnsAsync(_businessRules);
            
            Assert.IsFalse(_cacheManager.TryGetValue("ruleCacheKey", out RuleResponseDto emptyResponse));
            var result = await _ruleService.GetRulesAsync();
            
            _businessRuleRepositoryMock.Verify(x => x.GetAllBusinessRules(), Times.Once);
            Assert.AreEqual(_ruleResponseDto.RuleListJson, result.RuleListJson);
        }

    }
}