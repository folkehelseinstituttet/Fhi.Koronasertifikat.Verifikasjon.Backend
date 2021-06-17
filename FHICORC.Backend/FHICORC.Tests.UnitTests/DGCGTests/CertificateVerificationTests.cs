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
using System.Collections.Generic;
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
        private readonly ILogger<CertificateVerification> _nullLogger = new NullLoggerFactory().CreateLogger<CertificateVerification>();
        private readonly Mock<CertificateOptions> _mockCertificateOptions = new Mock<CertificateOptions>();
        private DgcgTrustListResponseDto _fullTestTrustList;
        private DgcgTrustListResponseDto _invalidTrustList; 
        private X509Certificate2 _trustAnchor;
        private DgcgTrustListItem _cscaTrustListItem;
        private DgcgTrustListItem _invalidCscaTrustListItem;


        [SetUp]
        public void Setup()
        {
            var parsedResponse = JsonConvert.DeserializeObject<DgcgTrustListItem[]>(File.ReadAllText("TestFiles/tst_full_trustlist_response.json"));
            _fullTestTrustList = new DgcgTrustListResponseDto { TrustListItems = parsedResponse.ToList() };

            var parsedResponseInvalid = JsonConvert.DeserializeObject<DgcgTrustListItem[]>(File.ReadAllText("TestFiles/tst_invalid_response.json"));
            _invalidTrustList = new DgcgTrustListResponseDto { TrustListItems = parsedResponseInvalid.ToList() };

            _mockCertificateOptions.Object.DGCGTrustAnchorPath = "Certificates/ta_tst.pem";
            _trustAnchor = new X509Certificate2(_mockCertificateOptions.Object.DGCGTrustAnchorPath);

            //Get one CSCA certificate
            var cscaTrustListItems = _fullTestTrustList.TrustListItems.FindAll(x => x.certificateType == CertificateType.CSCA.ToString());
            _cscaTrustListItem = cscaTrustListItems.OrderByDescending(x => x.timestamp).First();

            var cscaTrustListItems2 = _invalidTrustList.TrustListItems.FindAll(x => x.certificateType == CertificateType.CSCA.ToString());
            _invalidCscaTrustListItem = cscaTrustListItems2.OrderByDescending(x => x.timestamp).First();
        }

        [Test]
        public void Valid_CSCA_Verifies_Against_TrustAnchor()
        {
            var certificateVerification = new CertificateVerification(_nullLogger);

            var response = certificateVerification.VerifyItemByAnchorSignature(_cscaTrustListItem, _trustAnchor, "TrustAnchor");

            Assert.True(response); 
            Assert.NotNull(response);
        }

        [Test]
        public void Valid_Upload_Verifies_Against_TrustAnchor()
        {
            var certificateVerification = new CertificateVerification(_nullLogger);
            var uploadTrustListItem = _fullTestTrustList.TrustListItems.Find(x => x.certificateType == CertificateType.UPLOAD.ToString());

            var response = certificateVerification.VerifyItemByAnchorSignature(uploadTrustListItem, _trustAnchor, "TrustAnchor");

            Assert.True(response);
            Assert.NotNull(response);
        }

        [Test]
        public void Invalid_CSCA_DoesNotVerify__Against_TrustAnchor()
        {
            var certificateVerification = new CertificateVerification(_nullLogger);

            var response = certificateVerification.VerifyItemByAnchorSignature(_invalidCscaTrustListItem, _trustAnchor, "TrustAnchor");

            Assert.False(response);
            Assert.NotNull(response);
        }

        [Test]
        public void Incorrect_TrustAnchor_DoesNotVerify_Against_Valid_CSCA()
        {
            var certificateVerification = new CertificateVerification(_nullLogger);

            _trustAnchor = BuildSelfSignedCertificate("DifferentTA");

            var response = certificateVerification.VerifyItemByAnchorSignature(_cscaTrustListItem, _trustAnchor, "TrustAnchor");

            Assert.False(response);
            Assert.NotNull(response);
        }

        [Test] 
        public void DSC_Verified_By_Corresponding_Country_Upload()
        {
            var certificateVerification = new CertificateVerification(_nullLogger);

            var dscTrustListItem = _fullTestTrustList.TrustListItems.Find(x => x.country == "DE" && x.certificateType == CertificateType.DSC.ToString());
            var uploadTrustListItem = _fullTestTrustList.TrustListItems.Find(x => x.country == "DE" && x.certificateType == CertificateType.UPLOAD.ToString());
            var uploadCert = new X509Certificate2(Convert.FromBase64String(uploadTrustListItem.rawData));

            var response = certificateVerification.VerifyItemByAnchorSignature(dscTrustListItem, uploadCert, "Upload");

            Assert.IsNotNull(response);
            Assert.True(response); 
        }

        [Test]
        public void DSC_NotVerified_By_Incorrect_Country_Upload()
        {
            var certificateVerification = new CertificateVerification(_nullLogger);

            var dscTrustListItem = _fullTestTrustList.TrustListItems.Find(x => x.country == "DE" && x.certificateType == CertificateType.DSC.ToString());
            var uploadTrustListItem = _fullTestTrustList.TrustListItems.Find(x => x.country == "HR" && x.certificateType == CertificateType.UPLOAD.ToString());
            var uploadCert = new X509Certificate2(Convert.FromBase64String(uploadTrustListItem.rawData));

            var response = certificateVerification.VerifyItemByAnchorSignature(dscTrustListItem, uploadCert, "Upload");

            Assert.IsNotNull(response);
            Assert.False(response);
        }

        [Test]
        public void DSC_Verified_By_CSCA()
        {
            var certificateVerification = new CertificateVerification(_nullLogger);

            var cscaTrustListItemDe = _fullTestTrustList.TrustListItems.Find(x => x.country == "IT" && x.certificateType == CertificateType.CSCA.ToString());
            var dscItemList = _fullTestTrustList.TrustListItems.Find(x => x.country == "IT" && x.certificateType == CertificateType.DSC.ToString());
            var cscaCertList = new List<X509Certificate2> {new X509Certificate2(Convert.FromBase64String(cscaTrustListItemDe.rawData))};

            var response = certificateVerification.VerifyDscSignedByCsca(dscItemList, cscaCertList);

            Assert.IsNotNull(response);
            Assert.True(response);
        }

        [Test]
        public void DSC_NotVerified_By_CSCA()
        {
            var certificateVerification = new CertificateVerification(_nullLogger);

            var cscaTrustListItemDe = _fullTestTrustList.TrustListItems.Find(x => x.country == "SE" && x.certificateType == CertificateType.CSCA.ToString());
            var dscItemList = _fullTestTrustList.TrustListItems.Find(x => x.country == "DE" && x.certificateType == CertificateType.DSC.ToString());
            var cscaCertList = new List<X509Certificate2> {new X509Certificate2(Convert.FromBase64String(cscaTrustListItemDe.rawData))};

            var response = certificateVerification.VerifyDscSignedByCsca(dscItemList, cscaCertList);

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