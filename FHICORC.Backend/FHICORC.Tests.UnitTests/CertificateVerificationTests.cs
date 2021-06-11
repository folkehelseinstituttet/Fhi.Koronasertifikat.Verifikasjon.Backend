using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Integrations.DGCGateway.Models;
using FHICORC.Integrations.DGCGateway.Util;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace FHICORC.Tests.UnitTests
{
    [Category("Unit")]
    public class CertificateVerificationTests
    {
        ILogger<CertificateVerification> nullLogger = new NullLoggerFactory().CreateLogger<CertificateVerification>();
        private readonly Mock<CertificateOptions> mockCertificateOptions = new Mock<CertificateOptions>();
        private DgcgTrustListResponseDto fullTestTrustList;
        private DgcgTrustListResponseDto invalidTrustList; 
        private X509Certificate2 trustAnchor;
        private DgcgTrustListItem CSCATrustListItem;
        private DgcgTrustListItem InvalidCSCATrustListItem;


        [SetUp]
        public void Setup()
        {
            var parsedResponse = JsonConvert.DeserializeObject<DgcgTrustListItem[]>(File.ReadAllText("TestFiles/tst_full_trustlist_response.json"));
            fullTestTrustList = new DgcgTrustListResponseDto { TrustListItems = parsedResponse.ToList() };

            var parsedResponseInvalid = JsonConvert.DeserializeObject<DgcgTrustListItem[]>(File.ReadAllText("TestFiles/tst_invalid_response.json"));
            invalidTrustList = new DgcgTrustListResponseDto { TrustListItems = parsedResponseInvalid.ToList() };

            mockCertificateOptions.Object.DGCGTrustAnchorPath = "Certificates/ta_tst.pem";
            trustAnchor = new X509Certificate2(mockCertificateOptions.Object.DGCGTrustAnchorPath);

            //Get one CSCA certificate
            var CSCATrustListItems = fullTestTrustList.TrustListItems.FindAll(x => x.certificateType == CertificateType.CSCA.ToString());
            CSCATrustListItem = CSCATrustListItems.OrderByDescending(x => x.timestamp).First();

            var CSCATrustListItems2 = invalidTrustList.TrustListItems.FindAll(x => x.certificateType == CertificateType.CSCA.ToString());
            InvalidCSCATrustListItem = CSCATrustListItems2.OrderByDescending(x => x.timestamp).First();
        }

        [Test]
        public void Valid_CSCA_Verifies_Against_TrustAnchor()
        {
            var certificateVerification = new CertificateVerification(nullLogger);

            var response = certificateVerification.VerifyItemByAnchorSignature(CSCATrustListItem, trustAnchor, "TrustAnchor");

            Assert.True(response); 
            Assert.NotNull(response);
        }

        [Test]
        public void Valid_Upload_Verifies_Against_TrustAnchor()
        {
            var certificateVerification = new CertificateVerification(nullLogger);
            var UploadTrustListItem = fullTestTrustList.TrustListItems.Find(x => x.certificateType == CertificateType.UPLOAD.ToString());

            var response = certificateVerification.VerifyItemByAnchorSignature(UploadTrustListItem, trustAnchor, "TrustAnchor");

            Assert.True(response);
            Assert.NotNull(response);
        }

        [Test]
        public void Invalid_CSCA_DoesNotVerify__Against_TrustAnchor()
        {
            var certificateVerification = new CertificateVerification(nullLogger);

            var response = certificateVerification.VerifyItemByAnchorSignature(InvalidCSCATrustListItem, trustAnchor, "TrustAnchor");

            Assert.False(response);
            Assert.NotNull(response);
        }

        [Test]
        public void Incorrect_TrustAnchor_DoesNotVerify_Against_Valid_CSCA()
        {
            var certificateVerification = new CertificateVerification(nullLogger);

            trustAnchor = BuildSelfSignedCertificate("DifferentTA");

            var response = certificateVerification.VerifyItemByAnchorSignature(CSCATrustListItem, trustAnchor, "TrustAnchor");

            Assert.False(response);
            Assert.NotNull(response);
        }

        [Test] 
        public void DSC_Verified_By_Corresponding_Country_Upload()
        {
            var certificateVerification = new CertificateVerification(nullLogger);

            var DSCTrustListItem = fullTestTrustList.TrustListItems.Find(x => x.country == "DE" && x.certificateType == CertificateType.DSC.ToString());
            var UploadTrustListItem = fullTestTrustList.TrustListItems.Find(x => x.country == "DE" && x.certificateType == CertificateType.UPLOAD.ToString());
            var uploadCert = new X509Certificate2(Convert.FromBase64String(UploadTrustListItem.rawData));

            var response = certificateVerification.VerifyItemByAnchorSignature(DSCTrustListItem, uploadCert, "Upload");

            Assert.IsNotNull(response);
            Assert.True(response); 
        }

        [Test]
        public void DSC_NotVerified_By_Incorrect_Country_Upload()
        {
            var certificateVerification = new CertificateVerification(nullLogger);

            var DSCTrustListItem = fullTestTrustList.TrustListItems.Find(x => x.country == "DE" && x.certificateType == CertificateType.DSC.ToString());
            var UploadTrustListItem = fullTestTrustList.TrustListItems.Find(x => x.country == "HR" && x.certificateType == CertificateType.UPLOAD.ToString());
            var uploadCert = new X509Certificate2(Convert.FromBase64String(UploadTrustListItem.rawData));

            var response = certificateVerification.VerifyItemByAnchorSignature(DSCTrustListItem, uploadCert, "Upload");

            Assert.IsNotNull(response);
            Assert.False(response);
        }

        [Test]
        public void DSC_Verified_By_CSCA()
        {
            var certificateVerification = new CertificateVerification(nullLogger);

            var CSCATrustListItemDE = fullTestTrustList.TrustListItems.Find(x => x.country == "IT" && x.certificateType == CertificateType.CSCA.ToString());
            var DSCItemList = fullTestTrustList.TrustListItems.Find(x => x.country == "IT" && x.certificateType == CertificateType.DSC.ToString());
            var CSCACert = new X509Certificate2(Convert.FromBase64String(CSCATrustListItemDE.rawData));

            var response = certificateVerification.VerifyDscSignedByCsca(DSCItemList, CSCACert);

            Assert.IsNotNull(response);
            Assert.True(response);
        }

        [Test]
        public void DSC_NotVerified_By_CSCA()
        {
            var certificateVerification = new CertificateVerification(nullLogger);

            var CSCATrustListItemDE = fullTestTrustList.TrustListItems.Find(x => x.country == "SE" && x.certificateType == CertificateType.CSCA.ToString());
            var DSCItemList = fullTestTrustList.TrustListItems.Find(x => x.country == "DE" && x.certificateType == CertificateType.DSC.ToString());
            var CSCACert = new X509Certificate2(Convert.FromBase64String(CSCATrustListItemDE.rawData));

            var response = certificateVerification.VerifyDscSignedByCsca(DSCItemList, CSCACert);

            Assert.IsNotNull(response);
            Assert.False(response);
        }

        private X509Certificate2 BuildSelfSignedCertificate(string certificateName)
        {
            SubjectAlternativeNameBuilder sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddIpAddress(IPAddress.Loopback);
            sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
            sanBuilder.AddDnsName("localhost");
            sanBuilder.AddDnsName(Environment.MachineName);

            X500DistinguishedName distinguishedName = new X500DistinguishedName($"CN={certificateName}");

            using (RSA rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);

                request.CertificateExtensions.Add(
                    new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature , false));


                request.CertificateExtensions.Add(
                    new X509EnhancedKeyUsageExtension(
                        new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));

                request.CertificateExtensions.Add(sanBuilder.Build());

                var certificate= request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));
                certificate.FriendlyName = certificateName;

                return new X509Certificate2(certificate.Export(X509ContentType.Pfx, "WeNeedASaf3rPassword"), "WeNeedASaf3rPassword", X509KeyStorageFlags.MachineKeySet);
            }
        }
    }
}