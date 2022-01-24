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
    public class VaccineCodesServiceUnitTests
    {
        private readonly Mock<ILogger<TrustedIssuerService>> logger = new Mock<ILogger<TrustedIssuerService>>();
        private readonly Mock<IVaccineCodesRepository> vaccineCodesRepository = new Mock<IVaccineCodesRepository>();

        private VaccineCodesService service;

        [SetUp]
        public void Setup()
        {
            service = new VaccineCodesService(
                logger.Object,
                vaccineCodesRepository.Object);
        }

        [Test]
        public async Task ReplaceAutomaticVaccines_CallsRepository()
        {
            string codingSystem = "system1";

            await service.ReplaceAutomaticVaccines(new List<VaccineCodesModel>(), codingSystem);

            vaccineCodesRepository.Verify(x => x.ReplaceAutomaticVaccines(
                It.IsAny<IEnumerable<VaccineCodesModel>>(), codingSystem), Times.Once);
        }

        [Test]
        public async Task GetVaccinationInfo_CallsRepository()
        {
            ShcCodeRequestDto vaccineCode = new ShcCodeRequestDto()
            {
                Codes = new ShcCodeEntries[]
            {
                new ShcCodeEntries() { Code = "207", System = CodingSystem.Cvx } }
            };

            VaccineCodesModel vaccine = await service.GetVaccinationInfo(vaccineCode);

            vaccineCodesRepository.Verify(x => x.GetVaccInfo(
                It.IsAny<VaccineCodeKey>()), Times.Once);
        }

        [Test]
        public async Task RemoveAllVaccineCodes_CallsRepository()
        {
            bool onlyAuto = true;

            await service.RemoveAllVaccineCodes(onlyAuto);

            vaccineCodesRepository.Verify(x => x.CleanTable(onlyAuto), Times.Once);
        }

        [Test]
        public async Task RemoveVaccineCode_CallsRepository()
        {
            VaccineCodeKey vaccineCode = new VaccineCodeKey();

            await service.RemoveVaccineCode(vaccineCode);

            vaccineCodesRepository.Verify(x => x.RemoveVaccineCode(vaccineCode), Times.Once);
        }
    }
}
