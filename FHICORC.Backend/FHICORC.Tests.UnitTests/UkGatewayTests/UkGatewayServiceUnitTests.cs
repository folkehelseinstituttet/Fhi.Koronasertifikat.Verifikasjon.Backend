using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Repositories.Enums;
using FHICORC.Integrations.UkGateway.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace FHICORC.Tests.UnitTests.UkGatewayTests
{
    [Category("Unit")]
    public class UkGatewayServiceUnitTests
    {
        private const string DummyEndpoint = "https://dummy.example.com/pubkeys/keys.json";
        private readonly ILogger<UkGatewayService> _nullLogger = new NullLoggerFactory().CreateLogger<UkGatewayService>();

        [Test]
        public async Task CheckTrustListIsReturnedFromTest()
        {
            // ARRANGE
            var serviceEndpoints = new ServiceEndpoints { UKTrustListEndpoint = DummyEndpoint };
            var httpClientFactory = MockHttpClient(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[{'kid':'1','publicKey':'1234'},{'kid':'2','publicKey':'4321'}]")
            });
            var service = new UkGatewayService(httpClientFactory, serviceEndpoints, _nullLogger);

            // ACT
            var list = await service.GetTrustListAsync(SpecialCountryCodes.UK);

            // ASSERT
            Assert.Multiple(() =>
            {
                Assert.NotNull(list);
                Assert.AreEqual(2, list.Count);
                var entry1 = list.FirstOrDefault(e => e.KeyIdentifier == "1");
                Assert.NotNull(entry1);
                Assert.AreEqual("1234", entry1.PublicKey);
                Assert.AreEqual("UK", entry1.Country);
                Assert.AreEqual("DSC", entry1.CertificateType);
                var entry2 = list.FirstOrDefault(e => e.KeyIdentifier == "2");
                Assert.NotNull(entry2);
                Assert.AreEqual("4321", entry2.PublicKey);
                Assert.AreEqual("UK", entry2.Country);
                Assert.AreEqual("DSC", entry2.CertificateType);
            });
        }

        [Test]
        public void ThrowsOnCommunicationErrorTest()
        {
            // ARRANGE
            var serviceEndpoints = new ServiceEndpoints { NITrustListEndpoint = DummyEndpoint };
            var httpClientFactory = MockHttpClient(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.InternalServerError
            });
            var service = new UkGatewayService(httpClientFactory, serviceEndpoints, _nullLogger);

            // ACT/ASSERT
            Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await service.GetTrustListAsync(SpecialCountryCodes.UK_NI);
            });
        }

        private IHttpClientFactory MockHttpClient(HttpResponseMessage response)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            handlerMock 
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);
 
            var httpClient = new HttpClient(handlerMock .Object)
            {
                BaseAddress = new Uri(DummyEndpoint),
            };

            var factoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            factoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            return factoryMock.Object;
        }
    }
}
