using System.Threading.Tasks;
using FHICORC.Application.Common.Interfaces;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Application.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FHICORC.Tests.UnitTests.Services
{
    public class SHCServiceUnitTests
    {
        private readonly Mock<ICacheManager> cacheManager = new();
        private readonly Mock<ShcCacheOptions> cacheOptions = new();
        private readonly Mock<ILogger<SHCService>> logger = new();
        private readonly Mock<IMetricLogService> metricLogService = new();
        private readonly Mock<IBusinessRuleRepository> businessRuleRepository = new();

        private SHCService service;

        [SetUp]
        public void Setup()
        {
            service = new SHCService(
                cacheManager.Object,
                cacheOptions.Object,
                logger.Object,
                metricLogService.Object,
                businessRuleRepository.Object);
        }

        [Test]
        public async Task GetVaccinationInfosync_ValidCvxCode_ReturnsVaccineInfoAsync()
        {
            ShcCodeRequestDto vaccineCode = new()
            {
                Codes = new ShcCodeEntries[]
            {
                new ShcCodeEntries() { Code = "207", System = CodingSystem.Cvx } }
            };

            ShcVaccineResponseDto vaccine = await service.GetVaccinationInfosync(vaccineCode);

            Assert.NotNull(vaccine.Manufacturer);
            Assert.NotNull(vaccine.Name);
            Assert.NotNull(vaccine.Target);
            Assert.NotNull(vaccine.Type);
        }

        [Test]
        public async Task GetVaccinationInfosync_InalidCvxCode_ReturnsUnknownAsync()
        {
            ShcCodeRequestDto vaccineCode = new() { Codes = new ShcCodeEntries[]
            {
                new ShcCodeEntries() { Code = "12345lll", System = CodingSystem.Cvx } }
            };

            ShcVaccineResponseDto vaccine = await service.GetVaccinationInfosync(vaccineCode);

            Assert.Null(vaccine.Manufacturer);
            Assert.NotNull(vaccine.Name); // Unknown
            Assert.Null(vaccine.Target);
            Assert.Null(vaccine.Type);
        }

        [Test]
        public async Task GetIsTrustedsync_ValidIss_ReturnsTrustedAsync()
        {
            string iss = "https://ekeys.ny.gov/epass/doh/dvc/2021";
            ShcTrustRequestDto issuer = new() { Iss = iss };

            ShcTrustResponseDto trust = await service.GetIsTrustedsync(issuer);

            Assert.True(trust.Trusted);
        }

        [Test]
        public async Task GetIsTrustedsync_InvalidIss_ReturnsNotTrustedAsync()
        {
            string iss = "https://some-funny-website.com";
            ShcTrustRequestDto issuer = new() { Iss = iss };

            ShcTrustResponseDto trust = await service.GetIsTrustedsync(issuer);

            Assert.False(trust.Trusted);
        }
    }
}
