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
        private readonly Mock<ICacheManager> cacheManager = new Mock<ICacheManager>();
        private readonly Mock<ShcCacheOptions> cacheOptions = new Mock<ShcCacheOptions>();
        private readonly Mock<ILogger<TrustedIssuerService>> logger = new Mock<ILogger<TrustedIssuerService>>();
        private readonly Mock<IMetricLogService> metricLogService = new Mock<IMetricLogService>();
        private readonly Mock<ITrustedIssuerRepository> trustedIssuerRepository = new Mock<ITrustedIssuerRepository>();

        private TrustedIssuerService service;

        [SetUp]
        public void Setup()
        {
            service = new TrustedIssuerService(
                cacheManager.Object,
                cacheOptions.Object,
                logger.Object,
                metricLogService.Object,
                trustedIssuerRepository.Object);
        }

        [Test]
        public async Task GetVaccinationInfosync_ValidCvxCode_ReturnsVaccineInfoAsync()
        {
            ShcCodeRequestDto vaccineCode = new ShcCodeRequestDto()
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
            ShcCodeRequestDto vaccineCode = new ShcCodeRequestDto() { Codes = new ShcCodeEntries[]
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
            ShcTrustRequestDto issuer = new ShcTrustRequestDto() { iss = iss };

            ShcTrustResponseDto trust = await service.GetIsTrustedsync(issuer);

            Assert.True(trust.Trusted);
        }

        [Test]
        public async Task GetIsTrustedsync_InvalidIss_ReturnsNotTrustedAsync()
        {
            string iss = "https://some-funny-website.com";
            ShcTrustRequestDto issuer = new ShcTrustRequestDto() { iss = iss };

            ShcTrustResponseDto trust = await service.GetIsTrustedsync(issuer);

            Assert.False(trust.Trusted);
        }
    }
}
