using System;
using System.Linq;
using FHICORC.Core.Services.Enum;
using FHICORC.Integrations.DGCGateway.Util;
using NUnit.Framework;

namespace FHICORC.Tests.ComponentTests.DGCGComponentTests
{
    [Category("Component")]
    public class MobileDemoTests : SeedDb
    {
      

        [TestCase("J4PCK4sFs63kH/EeP7+C3A==")]
        //[TestCase("YWFhYWFhYWFhYWFhYWFhYQ==")]
        //[TestCase("p8dtvN0/9YKtwgGAxvsvBg==")]
        public void IsSingleHashInSuperFilter(string str) {
            var result = MobileUtil.ContainsCertificateFilterMobile("RO", str, _coronapassContext, bloomBucketService.GetBloomFilterBucket());

            Assert.True(result);
        }

        [Test]
        public void AreAllHashesInSuperFilter()
        {
            var cnt = 0;
            foreach (var hash in _coronapassContext.RevocationHash) {
                var country = _coronapassContext.RevocationBatch.Find(hash.BatchId).Country;
                cnt += 1;
                if (hash.Hash == "YWFhYWFhYWFhYWFhYWFhYQ==") {
                    var a = 0;
                }
                var result = MobileUtil.ContainsCertificateFilterMobile(country, hash.Hash, _coronapassContext, bloomBucketService.GetBloomFilterBucket());
                Assert.True(result);
            }
        }


        [Test]
        public void AddToDatabaseTest()
        {


            var str = "UCICountry";
            //Enum.TryParse("Active", out myHashType);
            var a = (int)(Enum.TryParse(str.ToUpper(), out HashType myHashType) ? myHashType : 0);




            //Assert.AreEqual(_coronapassContext.RevocationBatch.Count(), 102);
            //Assert.AreEqual(_coronapassContext.RevocationHash.Count(), 1638);
            //Assert.AreEqual(_coronapassContext.RevocationSuperFilter.Count(), 12);

        }


    }

}
