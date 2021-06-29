using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Integrations.DGCGateway.Models;
using FHICORC.Integrations.DGCGateway.Services;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FHICORC.Tests.UnitTests.DGCGTests
{
    [Category("Unit")]
    public class DgcgServiceUnitTests
    {
        private readonly Mock<IDgcgClient> dgcgClientMock = new Mock<IDgcgClient>(); 
        ILogger<DgcgService> nullLogger = new NullLoggerFactory().CreateLogger<DgcgService>();
        private readonly Mock<FeatureToggles> mockFeatureToggles = new Mock<FeatureToggles>();
        private readonly Mock<IDgcgResponseVerification> mockResponseVerification = new Mock<IDgcgResponseVerification>();

        [SetUp]
        public void Setup()
        {
            mockFeatureToggles.Object.DisableTrustListVerification = true;
            var dGCGTrustListResponseDto = new DgcgTrustListResponseDto();
            dGCGTrustListResponseDto.TrustListItems = new List<DgcgTrustListItem>();
            dgcgClientMock.Setup(x => x.FetchTrustListAsync(It.IsAny<string>())).ReturnsAsync(dGCGTrustListResponseDto);
            mockResponseVerification.Setup(x => x.VerifyResponseFromGateway(It.IsAny<DgcgTrustListResponseDto>())).Returns(dGCGTrustListResponseDto);

        }

        [Test]

        public async Task CheckTrustListIsReturnedFromTest()
        {
            var dGCGService = new DgcgService(nullLogger, dgcgClientMock.Object, mockResponseVerification.Object, mockFeatureToggles.Object);
            var response = await dGCGService.GetTrustListAsync(""); 

            dgcgClientMock.Verify(x => x.FetchTrustListAsync(""), Times.Once()); 

            Assert.NotNull(response);
            Assert.True(response.GetType().Equals(typeof(DgcgTrustListResponseDto))); 
        }
        [Test]
        public async Task CheckOnlyDSCIsReturned()
        {
            var dGCGService = new DgcgService(nullLogger, dgcgClientMock.Object, mockResponseVerification.Object, mockFeatureToggles.Object);
            var response = await dGCGService.GetTrustListAsync(CertificateType.DSC.ToString());

            dgcgClientMock.Verify(x => x.FetchTrustListAsync(CertificateType.DSC.ToString()), Times.Once());

            Assert.NotNull(response);
            Assert.True(response.GetType().Equals(typeof(DgcgTrustListResponseDto)));
        }

        [Test]
        public async Task CheckOnlyCSCAIsReturned()
        {
            var dGCGService = new DgcgService(nullLogger, dgcgClientMock.Object, mockResponseVerification.Object, mockFeatureToggles.Object);
            var response = await dGCGService.GetTrustListAsync(CertificateType.CSCA.ToString());

            dgcgClientMock.Verify(x => x.FetchTrustListAsync(CertificateType.CSCA.ToString()), Times.Once());

            Assert.NotNull(response);
            Assert.True(response.GetType().Equals(typeof(DgcgTrustListResponseDto)));
        }
        [Test]
        public async Task CheckOnlyUPLOADIsReturned()
        {
            var dGCGService = new DgcgService(nullLogger, dgcgClientMock.Object, mockResponseVerification.Object, mockFeatureToggles.Object);
            var response = await dGCGService.GetTrustListAsync(CertificateType.UPLOAD.ToString());

            dgcgClientMock.Verify(x => x.FetchTrustListAsync(CertificateType.UPLOAD.ToString()), Times.Once());

            Assert.NotNull(response);
            Assert.True(response.GetType().Equals(typeof(DgcgTrustListResponseDto)));
        }
        [Test]
        public async Task CheckOnlyAuthenticationIsReturned()
        {
            var dGCGService = new DgcgService(nullLogger, dgcgClientMock.Object, mockResponseVerification.Object, mockFeatureToggles.Object);
            var response = await dGCGService.GetTrustListAsync(CertificateType.AUTHENTICATION.ToString());

            dgcgClientMock.Verify(x => x.FetchTrustListAsync(CertificateType.AUTHENTICATION.ToString()), Times.Once());

            Assert.NotNull(response);
            Assert.True(response.GetType().Equals(typeof(DgcgTrustListResponseDto)));
        }
        [Test]
        public async Task Verification_Feature_Toggle_Works()
        {
            mockFeatureToggles.Object.DisableTrustListVerification = true;
            var dGCGService2 = new DgcgService(nullLogger, dgcgClientMock.Object, mockResponseVerification.Object, mockFeatureToggles.Object);
            var response2 = await dGCGService2.GetTrustListAsync("");

            mockResponseVerification.Verify(x => x.VerifyResponseFromGateway(response2), Times.Never());
            Assert.NotNull(response2);
            Assert.True(response2.GetType().Equals(typeof(DgcgTrustListResponseDto)));

            mockFeatureToggles.Object.DisableTrustListVerification = false;
            var dGCGService = new DgcgService(nullLogger, dgcgClientMock.Object, mockResponseVerification.Object, mockFeatureToggles.Object);
            var response = await dGCGService.GetTrustListAsync("");

            mockResponseVerification.Verify(x => x.VerifyResponseFromGateway(response), Times.Once());
            Assert.NotNull(response);
            Assert.True(response.GetType().Equals(typeof(DgcgTrustListResponseDto)));
        }

    }
}