using System.Collections.Generic;
using System.IO;
using System.Linq;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Domain.Models;
using FHICORC.Integrations.DGCGateway.Models;
using FHICORC.Integrations.DGCGateway.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace FHICORC.Tests.UnitTests.DGCGTests
{
    [Category("Unit")]
    public class DgcgResponseParserTests
    {
        ILogger<DgcgResponseParser> nullLogger = new NullLoggerFactory().CreateLogger<DgcgResponseParser>();
        private readonly Mock<FeatureToggles> featureTogglesMock = new Mock<FeatureToggles>();
        private DgcgTrustListResponseDto fullTestTrustList;
        private List<DgcgTrustListItem> DSCTrustListItems = new List<DgcgTrustListItem>();
        private DgcgResponseParser dGCGResponseParser;
        private DgcgTrustListResponseDto singleTrustList;
        private DgcgTrustListItem DSCTrustListItem; 


        [SetUp]
        public void Setup()
        {
            var parsedResponse = JsonConvert.DeserializeObject<DgcgTrustListItem[]>(File.ReadAllText("TestFiles/tst_full_trustlist_response.json"));
            fullTestTrustList = new DgcgTrustListResponseDto { TrustListItems = parsedResponse.ToList() };

            dGCGResponseParser = new DgcgResponseParser(nullLogger);
            DSCTrustListItems = fullTestTrustList.TrustListItems.FindAll(x => x.certificateType == CertificateType.DSC.ToString());

            //Single TrustList Item
            singleTrustList = new DgcgTrustListResponseDto();
            DSCTrustListItem = fullTestTrustList.TrustListItems.Find(x => x.certificateType == CertificateType.DSC.ToString());
            singleTrustList.TrustListItems = new List<DgcgTrustListItem>();
            singleTrustList.TrustListItems.Add(DSCTrustListItem);
        }

        [Test]
        public void All_DSCS_Parse_Successfully()
        {
            var response = dGCGResponseParser.ParseToEuDocSignerCertificate(fullTestTrustList);

            Assert.NotNull(response);
            Assert.True(response.GetType().Equals(typeof(List<EuDocSignerCertificate>)));
            Assert.True(response.Count.Equals(DSCTrustListItems.Count));
        }
        [Test]
        public void All_Fields_Parsed_Successfully()
        {
            var response = dGCGResponseParser.ParseToEuDocSignerCertificate(singleTrustList);

            Assert.NotNull(response);
            Assert.True(response[0].KeyIdentifier.Equals(DSCTrustListItem.kid));
            Assert.True(response[0].Country.Equals(DSCTrustListItem.country));
            Assert.True(response[0].CertificateType.Equals(DSCTrustListItem.certificateType));
            Assert.True(response[0].Thumbprint.Equals(DSCTrustListItem.thumbprint));
            Assert.True(response[0].Signature.Equals(DSCTrustListItem.signature));
            Assert.True(response[0].Certificate.Equals(DSCTrustListItem.rawData));
            Assert.True(response[0].Timestamp.Equals(DSCTrustListItem.timestamp));
        }

        [Test]
        public void Public_Key_Extracted_Successfully()
        {
            var response = dGCGResponseParser.ParseToEuDocSignerCertificate(singleTrustList);
            Assert.NotNull(response[0].PublicKey);
            Assert.True(response[0].PublicKey != DSCTrustListItem.rawData); 
        }

        [Test]
        public void Invalid_Public_Key_Not_Extracted()
        {
            singleTrustList.TrustListItems[0].rawData = "";
            try
            {
                var response = dGCGResponseParser.ParseToEuDocSignerCertificate(singleTrustList);
                Assert.Fail(); 
            }
            catch
            {
                Assert.Pass();
            }
        }
    }
}