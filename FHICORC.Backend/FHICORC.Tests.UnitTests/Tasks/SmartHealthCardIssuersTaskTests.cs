using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Models.SmartHealthCard;
using FHICORC.Application.Services.Interfaces;
using FHICORC.ApplicationHost.Hangfire.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace FHICORC.Tests.UnitTests.Tasks
{
    [Category("Unit")]
    public class SmartHealthCardIssuersTaskTests
    {
        private readonly Mock<HttpMessageHandler> httpMessageHandler = new();
        private readonly Mock<IHttpClientFactory> httpClientFactory = new();
        private readonly Mock<ILogger<SmartHealthCardIssuersTask>> logger = new();
        private Mock<ITrustedIssuerService> trustedIssuerService;

        private SmartHealthCardIssuersTask task;

        [SetUp]
        public void Setup()
        {
            trustedIssuerService = new();
            SetHttpResponse(new HttpResponseMessage());
            task = new SmartHealthCardIssuersTask(
                httpClientFactory.Object,
                logger.Object,
                trustedIssuerService.Object,
                new CronOptions(),
                new ServiceEndpoints { SHCIssuerListEndpoint = "https://dummy.example.com/pubkeys/keys.json" });
        }

        [Test]
        public void UpdateSmartHealthCardIssuers_HttpError_ThrowsException()
        {
            SetHttpResponse(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("")
            });

            Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await task.UpdateSmartHealthCardIssuers();
            });

            trustedIssuerService.Verify(x => x.ReplaceAutomaticallyAddedIssuers(It.IsAny<ShcIssuersDto>()), Times.Never);
        }

        [Test]
        public void UpdateSmartHealthCardIssuers_InvalidJson_ThrowsException()
        {
            SetHttpResponse(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"key\":\"value\"}")
            });

            Assert.ThrowsAsync<Exception>(async () =>
            {
                await task.UpdateSmartHealthCardIssuers();
            });

            trustedIssuerService.Verify(x => x.ReplaceAutomaticallyAddedIssuers(It.IsAny<ShcIssuersDto>()), Times.Never);
        }

        [Test]
        public void UpdateSmartHealthCardIssuers_WithZeroIssuers_ThrowsException()
        {
            SetHttpResponse(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"participating_issuers\":[]}")
            });

            Assert.ThrowsAsync<Exception>(async () =>
            {
                await task.UpdateSmartHealthCardIssuers();
            });

            trustedIssuerService.Verify(x => x.ReplaceAutomaticallyAddedIssuers(It.IsAny<ShcIssuersDto>()), Times.Never);
        }

        [Test]
        public void UpdateSmartHealthCardIssuers_WithIssuers_UpdatesRepository()
        {
            SetHttpResponse(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"participating_issuers\":[" +
                "{\"iss\":\"iss\", \"name\":\"name\" }" +
                "]}")
            });

            Assert.DoesNotThrowAsync(async () =>
            {
                await task.UpdateSmartHealthCardIssuers();
            });

            trustedIssuerService.Verify(x => x.ReplaceAutomaticallyAddedIssuers(It.IsAny<ShcIssuersDto>()), Times.Once);
        }

        [Test]
        public void UpdateSmartHealthCardIssuers_WithDuplicateIssuers_UpdatesRepositoryWithDistinct()
        {
            SetHttpResponse(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"participating_issuers\":[" +
                "{\"iss\":\"iss\", \"name\":\"name\" }," +
                 "{\"iss\":\"iss\", \"name\":\"name\" }" +
                "]}")
            });
            List<ShcIssuersDto> results = new();
            trustedIssuerService.Setup(h => h.ReplaceAutomaticallyAddedIssuers(Capture.In(results)));

            Assert.DoesNotThrowAsync(async () =>
            {
                await task.UpdateSmartHealthCardIssuers();
            });

            trustedIssuerService.Verify(x => x.ReplaceAutomaticallyAddedIssuers(It.IsAny<ShcIssuersDto>()), Times.Once);
            Assert.AreEqual(1, results[0].ParticipatingIssuers.Length);
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
