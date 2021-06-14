using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Integrations.DGCGateway.Models;
using FHICORC.Integrations.DGCGateway.Services;
using FHICORC.Integrations.DGCGateway.Util.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System;

namespace FHICORC.Tests.UnitTests
{
    [Category("Unit")]
    public class DgcgResponseVerificationTests
    {
        ILogger<DgcgResponseVerification> nullLogger = new NullLoggerFactory().CreateLogger<DgcgResponseVerification>();
        private readonly Mock<CertificateOptions> mockCertificateOptions = new Mock<CertificateOptions>();
        private readonly Mock<ICertificateVerification> certificateVerification = new Mock<ICertificateVerification>();
        private DgcgTrustListResponseDto fullTestTrustList;
        private DgcgResponseVerification dGCGResponseVerification;
        private DgcgTrustListItem CSCATrustListItem;
        private DgcgTrustListItem CSCATrustListItemDE;
        private DgcgTrustListItem UploadTrustListDE; 


        //TODO Move private functions into DGCGResponseVerification with public methods to be tested here 
        [SetUp]
        public void Setup()
        {
            var parsedResponse = JsonConvert.DeserializeObject<DgcgTrustListItem[]>(File.ReadAllText("TestFiles/tst_full_trustlist_response.json"));
            fullTestTrustList = new DgcgTrustListResponseDto { TrustListItems = parsedResponse.ToList() };
            dGCGResponseVerification = new DgcgResponseVerification(nullLogger, mockCertificateOptions.Object, certificateVerification.Object);

            var CSCATrustListItems = fullTestTrustList.TrustListItems.FindAll(x => x.certificateType == CertificateType.CSCA.ToString());
            CSCATrustListItem = CSCATrustListItems.OrderByDescending(x => x.timestamp).First();

            var CSCATrustListItemsDE = fullTestTrustList.TrustListItems.FindAll(x => x.country == "DE" && x.certificateType == CertificateType.CSCA.ToString());
            CSCATrustListItemDE = CSCATrustListItemsDE.OrderByDescending(x => x.timestamp).First();

            var UploadTrustListsDE = fullTestTrustList.TrustListItems.FindAll(x => x.country == "DE" && x.certificateType == CertificateType.UPLOAD.ToString());
            UploadTrustListDE = UploadTrustListsDE.OrderByDescending(x => x.timestamp).First();

            mockCertificateOptions.Object.DGCGTrustAnchorPath = "Certificates/ta_tst.pem";
        }

        [Test]
        public void All_Returned_Countries_Are_Verified()
        {
            var response = dGCGResponseVerification.VerifyResponseFromGateway(fullTestTrustList);
            var uploadCerts = fullTestTrustList.TrustListItems.Where(t => t.certificateType == CertificateType.UPLOAD.ToString()).ToList();
            var cscaCerts = fullTestTrustList.TrustListItems.Where(t => t.certificateType == CertificateType.CSCA.ToString()).ToList();

            Assert.NotNull(response);
            Assert.NotNull(uploadCerts.Count);
            Assert.NotNull(cscaCerts.Count);
            certificateVerification.Verify(x => x.VerifyItemByAnchorSignature(It.IsAny<DgcgTrustListItem>(), It.IsAny<X509Certificate2>(), It.IsAny<string>()), Times.Exactly(uploadCerts.Count + cscaCerts.Count));
        }
        [Test]
        public void TrustLists_NotAdded_If_Validation_Fails()
        {
            //Setup mock 
            certificateVerification.Setup(x => x.VerifyDscSignedByCsca(It.IsAny<DgcgTrustListItem>(), It.IsAny<X509Certificate2>())).Returns(false); 

            var DSCTrustListItemsDE = fullTestTrustList.TrustListItems.FindAll(x => x.country == "DE" && x.certificateType == CertificateType.DSC.ToString());
            var count = DSCTrustListItemsDE.Count;
            var fullListCount = fullTestTrustList.TrustListItems.Count;

            var cscaList = new List<X509Certificate2> { new X509Certificate2(Convert.FromBase64String(CSCATrustListItemDE.rawData)) };
            var uploadList = new List<X509Certificate2> { new X509Certificate2(Convert.FromBase64String(UploadTrustListDE.rawData)) };
            var response = dGCGResponseVerification.VerifyAndGetDscs(fullTestTrustList, cscaList, uploadList, "DE");

            Assert.True(response.Count == 0);
        }

        [Test]
        public void DSC_Not_Added_No_UploadCert()
        {
            var singleItem = new DgcgTrustListResponseDto();
            singleItem.TrustListItems = new List<DgcgTrustListItem>();
            var trustList = new DgcgTrustListItem() {
                certificateType = CertificateType.CSCA.ToString(),
                country = "DK"
            };
            singleItem.TrustListItems.Add(trustList); 

            var response = dGCGResponseVerification.VerifyResponseFromGateway(singleItem);
            Assert.True(response.TrustListItems.Count == 0);
        }

        [Test]
        public void TrustList_Removed_No_DSCCert()
        {
            //TODO
        }

        [Test]
        public void Every_DSC_Verified()
        {
            //TODO
        }

        [Test]
        public void DSC_Removed_Failed_VerifyDSCSignedByCSCA()
        {
            //TODO
        }

        [Test]
        public void DSC_Removed_Failed_VerifyDSCByUploadCertificate()
        {
            //TODO
        }
    }
}