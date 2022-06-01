using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Integrations.DGCGateway.Models;
using FHICORC.Integrations.DGCGateway.Services;
using FHICORC.Integrations.DGCGateway.Util;
using FHICORC.Integrations.DGCGateway.Util.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace FHICORC.Tests.UnitTests.DGCGTests
{
    [Category("Unit")]
    public class DgcgResponseVerificationTests
    {
        readonly ILogger<DgcgResponseVerification<X509Certificate2>> _nullLogger = new NullLoggerFactory().CreateLogger<DgcgResponseVerification<X509Certificate2>>();
        private readonly Mock<CertificateOptions> _mockCertificateOptions = new Mock<CertificateOptions>();
        private readonly Mock<ICertificateVerification<X509Certificate2>> _certificateVerification = new Mock<ICertificateVerification<X509Certificate2>>();
        private DgcgTrustListResponseDto _fullTestTrustList;
        private DgcgResponseVerification<X509Certificate2> _dgcgResponseVerification;
        private DgcgTrustListItem _cscaTrustListItemDe;
        private X509Certificate2 _cscaCertDe;
        private DgcgTrustListItem _uploadTrustListDe;
        private X509Certificate2 _uploadCertDe;
        private readonly Mock<ILogger<DgcgResponseVerification<X509Certificate2>>> _loggerMock = new Mock<ILogger<DgcgResponseVerification<X509Certificate2>>>();

        [SetUp]
        public void Setup()
        {
            var parsedResponse = JsonConvert.DeserializeObject<DgcgTrustListItem[]>(File.ReadAllText("TestFiles/tst_full_trustlist_response.json"));
            _fullTestTrustList = new DgcgTrustListResponseDto { TrustListItems = parsedResponse.ToList() };
            _dgcgResponseVerification = new DgcgResponseVerification<X509Certificate2>(_nullLogger, _mockCertificateOptions.Object, _certificateVerification.Object);

            var cscaTrustListItemsDE = _fullTestTrustList.TrustListItems.FindAll(x => x.country == "DE" && x.certificateType == CertificateType.CSCA.ToString());
            _cscaTrustListItemDe = cscaTrustListItemsDE.OrderByDescending(x => x.timestamp).First();
            _cscaCertDe = new X509Certificate2(Base64Util.FromString(_cscaTrustListItemDe.rawData));

            var uploadTrustListsDE = _fullTestTrustList.TrustListItems.FindAll(x => x.country == "DE" && x.certificateType == CertificateType.UPLOAD.ToString());
            _uploadTrustListDe = uploadTrustListsDE.OrderByDescending(x => x.timestamp).First();
            _uploadCertDe = new X509Certificate2(Base64Util.FromString(_uploadTrustListDe.rawData));


            _mockCertificateOptions.Object.DGCGTrustAnchorPath = "Certificates/local_ta.pem";
        }
        
        [Test]
        [Ignore ("Skip because Unit test is invalid - Inconsistent on different machines")]
        public void All_Returned_Countries_Are_Verified()
        {
            _certificateVerification.Setup(x => x.VerifyDscSignedByCsca(It.IsAny<DgcgTrustListItem>(), It.IsAny<List<X509Certificate2>>())).Returns(true);
            _certificateVerification.Setup(x => x.VerifyItemByAnchorSignature(It.IsAny<DgcgTrustListItem>(), It.IsAny<X509Certificate2>(), It.IsAny<string>())).Returns(true);

            var countries = _fullTestTrustList.TrustListItems.Select(x => x.country).Distinct().ToList();

            var response = _dgcgResponseVerification.VerifyResponseFromGateway(_fullTestTrustList);
            var responseCountries = response.TrustListItems.Select(x => x.country).Distinct().ToList();

            Assert.NotNull(response);
            Assert.NotNull(countries.Count);
            Assert.AreEqual(countries.Count, responseCountries.Count);
        }

        [Test]
        public void DSC_Not_Added_No_UploadCert()
        {
            _certificateVerification.Setup(x => x.VerifyItemByAnchorSignature(It.IsAny<DgcgTrustListItem>(), It.IsAny<X509Certificate2>(), It.IsAny<string>())).Returns(true);

            var singleItem = new DgcgTrustListResponseDto();
            singleItem.TrustListItems = new List<DgcgTrustListItem>();
            var trustListSingle = new DgcgTrustListItem() {
                certificateType = CertificateType.CSCA.ToString(),
                country = "DK",
                rawData = "MIICETCCAbagAwIBAgIJAMEwKd6bwdZPMAoGCCqGSM49BAMCMGoxCzAJBgNVBAYTAkRLMRUwEwYDVQQHDAxLQzNCOGJlbmhhdm4xEzARBgNVBAoMCk5ldGNvbXBhbnkxFDASBgNVBAsMC0RldmVsb3BtZW50MRkwFwYDVQQDDBAqLm5ldGNvbXBhbnkuY29tMB4XDTIxMDUxMzA4MTI1M1oXDTI1MDUxMzA4MTI1M1owajELMAkGA1UEBhMCREsxFTATBgNVBAcMDEtDM0I4YmVuaGF2bjETMBEGA1UECgwKTmV0Y29tcGFueTEUMBIGA1UECwwLRGV2ZWxvcG1lbnQxGTAXBgNVBAMMECoubmV0Y29tcGFueS5jb20wWTATBgcqhkjOPQIBBggqhkjOPQMBBwNCAAQZPz9NFNTLEikLBIwJoxrd4Dt15Z3uLnNveuYeSygiEZR8vheoOS8RdguQX1Kpnv8t2BZtrjVm2XStp3qHEWemo0UwQzASBgNVHRMBAf8ECDAGAQH/AgEAMA4GA1UdDwEB/wQEAwIBBjAdBgNVHQ4EFgQUViIgIm8dJNUilawqXRFaGCO0AW8wCgYIKoZIzj0EAwIDSQAwRgIhAI+K9/6A036BbzxhtaOzrfMXjty1X0oTOU8ZzT9tx2K/AiEAh3YePpvY+ZqhO53p2sMJRmRLJ5qDS7O9uPOrpZ9RSgg="
            };
            singleItem.TrustListItems.Add(trustListSingle); 

            var response = _dgcgResponseVerification.VerifyResponseFromGateway(singleItem);
            Assert.AreEqual(1, response.TrustListItems.Count);
        }

        [Test]
        public void All_DSCs_Added_If_Verification_Passed()
        {
            // ARRANGE
            _certificateVerification.Setup(x => x.VerifyDscSignedByCsca(It.IsAny<DgcgTrustListItem>(), It.IsAny<List<X509Certificate2>>())).Returns(true);
            _certificateVerification.Setup(x => x.VerifyItemByAnchorSignature(It.IsAny<DgcgTrustListItem>(), It.IsAny<List<X509Certificate2>>(), It.IsAny<string>())).Returns(true);

            var dscTrustListItemsDe = _fullTestTrustList.TrustListItems.FindAll(x => x.country == "DE" && x.certificateType == CertificateType.DSC.ToString());
            var dscDeCount = dscTrustListItemsDe.Count;

            // ACT
            var response = _dgcgResponseVerification.VerifyAndGetDscs(_fullTestTrustList,
                new List<X509Certificate2> {_cscaCertDe}, new List<X509Certificate2> {_uploadCertDe}, "DE");

            // ASSERT
            Assert.AreEqual(dscDeCount, response.Count);
        }

        [Test]
        public void Empty_List_Returned_No_DSC()
        {
            var singleItem = new DgcgTrustListResponseDto();
            singleItem.TrustListItems = new List<DgcgTrustListItem>();
            var trustList = new DgcgTrustListItem() {
                certificateType = CertificateType.CSCA.ToString(),
                country = "DK"
            };
            singleItem.TrustListItems.Add(trustList); 

            // Act
            var response = _dgcgResponseVerification.VerifyAndGetDscs(singleItem,
                new List<X509Certificate2> {_cscaCertDe}, new List<X509Certificate2> {_uploadCertDe}, "DK");

            // Assert
            Assert.IsEmpty(response);
        }

        [Test]
        public void DSC_NotAdded_Failed_VerifyDSCByUploadCertificate()
        {
            // ARRANGE
            _certificateVerification.Setup(x => x.VerifyDscSignedByCsca(It.IsAny<DgcgTrustListItem>(), It.IsAny<List<X509Certificate2>>())).Returns(false);

            var dscTrustListItemsDe = _fullTestTrustList.TrustListItems.FindAll(x => x.country == "DE" && x.certificateType == CertificateType.DSC.ToString());
            var dscDeCount = dscTrustListItemsDe.Count;

            // ACT
            var response = _dgcgResponseVerification.VerifyAndGetDscs(_fullTestTrustList,
                new List<X509Certificate2> {_cscaCertDe}, new List<X509Certificate2> {_uploadCertDe}, "DE");

            // ASSERT
            Assert.AreNotEqual(dscDeCount, response.Count);
        }

        [Test]
        public void DSC_NotAdded_Failed_VerifyDSCSignedByCSCA()
        {
            // ARRANGE
            _certificateVerification.Setup(x => x.VerifyDscSignedByCsca(It.IsAny<DgcgTrustListItem>(), It.IsAny<List<X509Certificate2>>())).Returns(true);
            _certificateVerification.Setup(x => x.VerifyItemByAnchorSignature(It.IsAny<DgcgTrustListItem>(), It.IsAny<List<X509Certificate2>>(), It.IsAny<string>())).Returns(false);

            var dscTrustListItemsDe = _fullTestTrustList.TrustListItems.FindAll(x => x.country == "DE" && x.certificateType == CertificateType.DSC.ToString());
            var dscDeCount = dscTrustListItemsDe.Count;

            // ACT
            var response = _dgcgResponseVerification.VerifyAndGetDscs(_fullTestTrustList,
                new List<X509Certificate2> {_cscaCertDe}, new List<X509Certificate2> {_uploadCertDe}, "DE");

            // ASSERT
            Assert.AreNotEqual(dscDeCount, response.Count);
        }

        [Test]
        public void CSCA_and_Upload_Added_To_Response()
        {
            // ARRANGE
            _certificateVerification.Setup(x => x.VerifyItemByAnchorSignature(It.IsAny<DgcgTrustListItem>(), It.IsAny<X509Certificate2>(), It.IsAny<string>())).Returns(true);

            var twoItems = new DgcgTrustListResponseDto();
            twoItems.TrustListItems = new List<DgcgTrustListItem>();
            var trustList1 = new DgcgTrustListItem()
            {
                certificateType = CertificateType.CSCA.ToString(),
                country = "DK",
                rawData = "MIIBvTCCAWOgAwIBAgIKAXk8i88OleLsuTAKBggqhkjOPQQDAjA2MRYwFAYDVQQDDA1BVCBER0MgQ1NDQSAxMQswCQYDVQQGEwJBVDEPMA0GA1UECgwGQk1TR1BLMB4XDTIxMDUwNTEyNDEwNloXDTIzMDUwNTEyNDEwNlowPTERMA8GA1UEAwwIQVQgRFNDIDExCzAJBgNVBAYTAkFUMQ8wDQYDVQQKDAZCTVNHUEsxCjAIBgNVBAUTATEwWTATBgcqhkjOPQIBBggqhkjOPQMBBwNCAASt1Vz1rRuW1HqObUE9MDe7RzIk1gq4XW5GTyHuHTj5cFEn2Rge37 + hINfCZZcozpwQKdyaporPUP1TE7UWl0F3o1IwUDAOBgNVHQ8BAf8EBAMCB4AwHQYDVR0OBBYEFO49y1ISb6cvXshLcp8UUp9VoGLQMB8GA1UdIwQYMBaAFP7JKEOflGEvef2iMdtopsetwGGeMAoGCCqGSM49BAMCA0gAMEUCIQDG2opotWG8tJXN84ZZqT6wUBz9KF8D + z9NukYvnUEQ3QIgdBLFSTSiDt0UJaDF6St2bkUQuVHW6fQbONd731/M4nc="
            };
            var trustList2 = new DgcgTrustListItem()
        {
                certificateType = CertificateType.UPLOAD.ToString(),
                country = "DK",
                rawData = "MIICPDCCAeKgAwIBAgIJAPLBOBHbFTuCMAoGCCqGSM49BAMCMIGJMQswCQYDVQQGEwJFRTERMA8GA1UECAwISGFyanVtYWExEDAOBgNVBAcMB1RhbGxpbm4xNjA0BgNVBAoMLUhlYWx0aCBhbmQgV2VsZmFyZSBJbmZvcm1hdGlvbiBTeXN0ZW1zIENlbnRlcjEdMBsGA1UEAwwUREdDX05CX1VQX1RFU1RfRUVfMDEwHhcNMjEwNTA2MTExNTQyWhcNMjUwNTA2MTExNTQyWjCBiTELMAkGA1UEBhMCRUUxETAPBgNVBAgMCEhhcmp1bWFhMRAwDgYDVQQHDAdUYWxsaW5uMTYwNAYDVQQKDC1IZWFsdGggYW5kIFdlbGZhcmUgSW5mb3JtYXRpb24gU3lzdGVtcyBDZW50ZXIxHTAbBgNVBAMMFERHQ19OQl9VUF9URVNUX0VFXzAxMFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAENrZt8Mxad3gwwy/VLOMKd1Nv+xYSB1xYg+uTe7jSOhRTClFVTgHrS/n4OfCuH4eJaAot3F+XcawHygmhSx0696MxMC8wDgYDVR0PAQH/BAQDAgeAMB0GA1UdDgQWBBQX5gASC3uwNYCa3ENN0Q/H3KV9QzAKBggqhkjOPQQDAgNIADBFAiBIuIfBRh30m3Eft8TCtNq7hedFLKk1/ZIVwNWAlNikUwIhANhUjkCI6BCquabiZLvZVp33LJ39yNos8+4UxsaHMl4g"

            };
            twoItems.TrustListItems.Add(trustList1);
            twoItems.TrustListItems.Add(trustList2);

            // ACT
            var response = _dgcgResponseVerification.VerifyResponseFromGateway(twoItems);

            // ASSERT
            Assert.AreEqual(2, response.TrustListItems.Count);
        }

        [Test]
        public void Country_Skipped_CSCANull()
        {
            _dgcgResponseVerification = new DgcgResponseVerification<X509Certificate2>(_loggerMock.Object, _mockCertificateOptions.Object, _certificateVerification.Object);

            var dgcgTrustListResponseDto = new DgcgTrustListResponseDto();
            dgcgTrustListResponseDto.TrustListItems = new List<DgcgTrustListItem>();
            var trustList1 = new DgcgTrustListItem()
            {
                certificateType = CertificateType.DSC.ToString(),
                country = "DK",
                rawData = "MIIBvTCCAWOgAwIBAgIKAXk8i88OleLsuTAKBggqhkjOPQQDAjA2MRYwFAYDVQQDDA1BVCBER0MgQ1NDQSAxMQswCQYDVQQGEwJBVDEPMA0GA1UECgwGQk1TR1BLMB4XDTIxMDUwNTEyNDEwNloXDTIzMDUwNTEyNDEwNlowPTERMA8GA1UEAwwIQVQgRFNDIDExCzAJBgNVBAYTAkFUMQ8wDQYDVQQKDAZCTVNHUEsxCjAIBgNVBAUTATEwWTATBgcqhkjOPQIBBggqhkjOPQMBBwNCAASt1Vz1rRuW1HqObUE9MDe7RzIk1gq4XW5GTyHuHTj5cFEn2Rge37 + hINfCZZcozpwQKdyaporPUP1TE7UWl0F3o1IwUDAOBgNVHQ8BAf8EBAMCB4AwHQYDVR0OBBYEFO49y1ISb6cvXshLcp8UUp9VoGLQMB8GA1UdIwQYMBaAFP7JKEOflGEvef2iMdtopsetwGGeMAoGCCqGSM49BAMCA0gAMEUCIQDG2opotWG8tJXN84ZZqT6wUBz9KF8D + z9NukYvnUEQ3QIgdBLFSTSiDt0UJaDF6St2bkUQuVHW6fQbONd731/M4nc="
            };
            dgcgTrustListResponseDto.TrustListItems.Add(trustList1);


            var response = _dgcgResponseVerification.VerifyResponseFromGateway(dgcgTrustListResponseDto);
            Assert.AreEqual(0, response.TrustListItems.Count);
            Assert.AreEqual(2, _loggerMock.Invocations.Count);
        }
    }
}