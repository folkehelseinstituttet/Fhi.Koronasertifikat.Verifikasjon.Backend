using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using FHICORC.Application.Models.SmartHealthCard;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Application.Services;
using FHICORC.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FHICORC.Tests.UnitTests.Services
{
    public class TrustedIssuerServiceUnitTests
    {
        private readonly Mock<ILogger<TrustedIssuerService>> logger = new Mock<ILogger<TrustedIssuerService>>();
        private readonly Mock<ITrustedIssuerRepository> trustedIssuerRepository = new Mock<ITrustedIssuerRepository>();

        private TrustedIssuerService service;

        [SetUp]
        public void Setup()
        {
            service = new TrustedIssuerService(
                logger.Object,
                trustedIssuerRepository.Object);
        }

        [Test]
        public void GetIssuer_ValidIss_ReturnsTrusted()
        {
            string iss = "https://ekeys.ny.gov/epass/doh/dvc/2021";
            trustedIssuerRepository.Setup(x => x.GetIssuer(iss)).Returns(new TrustedIssuerModel());

            TrustedIssuerModel trust = service.GetIssuer(iss);

            Assert.NotNull(trust);
        }

        [Test]
        public void GetIssuer_InvalidIss_ReturnsNotTrusted()
        {
            string otherIss = "https://some-funny-website.com";
            string iss = "https://ekeys.ny.gov/epass/doh/dvc/2021";
            trustedIssuerRepository.Setup(x => x.GetIssuer(iss)).Returns(new TrustedIssuerModel());

            TrustedIssuerModel trust = service.GetIssuer(otherIss);

            Assert.Null(trust);
        }

        [Test]
        public async Task AddIssuers_Manually_CallsRepositoryWithCorrectModelsAsync()
        {
            List<IEnumerable<TrustedIssuerModel>> results = new();
            trustedIssuerRepository.Setup(h => h.AddIssuers(Capture.In(results)));
            AddIssuersRequest request = new AddIssuersRequest()
            {
                issuers = new Issuer[]
                {
                    new Issuer(){ issuer = "issue", name = "name" }
                }
            };

            await service.AddIssuers(request, true);

            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(1, results.First().Count());
            Assert.True(results.First().First().IsAddManually);
        }

        [Test]
        public async Task AddIssuers_NotManually_CallsRepositoryWithCorrectModelsAsync()
        {
            List<IEnumerable<TrustedIssuerModel>> results = new();
            trustedIssuerRepository.Setup(h => h.AddIssuers(Capture.In(results)));
            AddIssuersRequest request = new AddIssuersRequest()
            {
                issuers = new Issuer[]
                {
                    new Issuer(){ issuer = "issue", name = "name" }
                }
            };

            await service.AddIssuers(request, false);

            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(1, results.First().Count());
            Assert.False(results.First().First().IsAddManually);
        }

        [Test]
        public async Task ReplaceAutomaticallyAddedIssuers_CallsRepositoryWithModelsAsync()
        {
            List<IEnumerable<TrustedIssuerModel>> results = new();
            trustedIssuerRepository.Setup(h => h.ReplaceAutomaticallyAddedIssuers(Capture.In(results)));
            ShcIssuersDto request = new ShcIssuersDto()
            {
                ParticipatingIssuers = new ShcIssuerDto[]
                {
                    new ShcIssuerDto(){ Iss = "issue", Name = "name" }
                }
            };

            await service.ReplaceAutomaticallyAddedIssuers(request);

            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(1, results.First().Count());
            Assert.False(results.First().First().IsAddManually);
        }

        [Test]
        public async Task RemoveIssuer_CallsRepository()
        {
            string iss = "iss1";

            await service.RemoveIssuer(iss);

            trustedIssuerRepository.Verify(x => x.RemoveIssuer(iss), Times.Once);
        }

        [Test]
        public async Task RemoveAllIssuers_CallsRepository()
        {
            bool keepIsAddManually = true;

            await service.RemoveAllIssuers(keepIsAddManually);

            trustedIssuerRepository.Verify(x => x.CleanTable(keepIsAddManually), Times.Once);
        }
    }
}
