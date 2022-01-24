using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Models.SmartHealthCard;
using FHICORC.Application.Services.Interfaces;
using FHICORC.ApplicationHost.Hangfire.Tasks;
using FHICORC.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace FHICORC.Tests.UnitTests.Tasks
{
    [Category("Unit")]
    public class SmartHealthCardVaccinesTaskTests
    {
        private readonly Mock<HttpMessageHandler> httpMessageHandler = new();
        private readonly Mock<IHttpClientFactory> httpClientFactory = new();
        private readonly Mock<ILogger<SmartHealthCardVaccinesTask>> logger = new();
        private Mock<IVaccineCodesService> vaccineCodesService;

        private SmartHealthCardVaccinesTask task;
    
        [SetUp]
        public void Setup()
        {
            vaccineCodesService = new();
            SetHttpResponse(new HttpResponseMessage());
            task = new SmartHealthCardVaccinesTask(
                httpClientFactory.Object,
                logger.Object,
                vaccineCodesService.Object,
                new CronOptions(),
                new ServiceEndpoints { SHCVaccineCvxListEndpoint = "https://dummy.example.com/pubkeys/keys.json" });
        }

        [Test]
        public void UpdateSmartHealthCardVaccines_HttpError_ThrowsException()
        {
            SetHttpResponse(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("")
            });

            Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await task.UpdateSmartHealthCardVaccines();
            });

            vaccineCodesService.Verify(x => x.ReplaceAutomaticVaccines(It.IsAny<IEnumerable<VaccineCodesModel>>(), CodingSystem.Cvx), Times.Never);
        }

        [Test]
        public void UpdateSmartHealthCardVaccines_WithZeroVaccines_ThrowsException()
        {
            SetHttpResponse(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Empty")
            });

            Assert.ThrowsAsync<Exception>(async () =>
            {
                await task.UpdateSmartHealthCardVaccines();
            });

            vaccineCodesService.Verify(x => x.ReplaceAutomaticVaccines(It.IsAny<IEnumerable<VaccineCodesModel>>(), CodingSystem.Cvx), Times.Never);
        }

        [Test]
        public void UpdateSmartHealthCardVaccines_WithVaccines_UpdatesRepository()
        {
            SetHttpResponse(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("hi|hi|here|is|a|row|")
            });

            Assert.DoesNotThrowAsync(async () =>
            {
                await task.UpdateSmartHealthCardVaccines();
            });

            vaccineCodesService.Verify(x => x.ReplaceAutomaticVaccines(It.IsAny<IEnumerable<VaccineCodesModel>>(), CodingSystem.Cvx), Times.Once);
        }

        [Test]
        public void UpdateSmartHealthCardVaccines_WithNonCovid19Vaccines_FiltersThem()
        {
            SetHttpResponse(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("COVID-19|hi|here|is|a|row|\n" +
                    "hi|hi|here|is|a|row|\n" +
                    "covid-19|hi|here|is|a|row|")
            });
            List<IEnumerable<VaccineCodesModel>> results = new();
            vaccineCodesService.Setup(h => h.ReplaceAutomaticVaccines(Capture.In(results), CodingSystem.Cvx));

            Assert.DoesNotThrowAsync(async () =>
            {
                await task.UpdateSmartHealthCardVaccines();
            });

            vaccineCodesService.Verify(x => x.ReplaceAutomaticVaccines(It.IsAny<IEnumerable<VaccineCodesModel>>(), CodingSystem.Cvx), Times.Once);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(2, results[0].Count());
        }

        private void SetHttpResponse(HttpResponseMessage response)
        {
            httpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response)
                .Verifiable();
            HttpClient httpClient = new(httpMessageHandler.Object);
            httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
        }
    }
}
