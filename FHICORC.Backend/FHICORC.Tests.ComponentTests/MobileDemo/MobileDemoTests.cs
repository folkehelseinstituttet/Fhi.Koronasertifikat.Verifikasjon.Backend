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
      

        //[TestCase("J4PCK4sFs63kH/EeP7+C3A==")]

        //[TestCase("YWFhYWFhYWFhYWFhYWFhYQ==")]
        //[TestCase("p8dtvN0/9YKtwgGAxvsvBg==")]
        [TestCase("+hkApE6qvfhfb95y/Jjx/w==")]
        public void IsSingleHashInSuperFilter(string str) {
            var superFilter = _coronapassContext.RevocationSuperFilter.Where(x => x.SuperCountry=="CZ").ToList();
            var result = MobileUtil.ContainsCertificateFilterMobile(str, str, superFilter, bloomBucketService.GetBloomFilterBucket().ToList());

            Assert.True(result);
        }

        [Test]
        public void AreAllHashesInSuperFilter()
        {
            var cnt = 0;
            var superFilter = _coronapassContext.RevocationSuperFilter.ToList();
            foreach (var hash in _coronapassContext.RevocationHash) {
                var country = _coronapassContext.RevocationBatch.Find(hash.BatchId).Country;
                cnt += 1;
                
                var result = MobileUtil.ContainsCertificateFilterMobile(hash.Hash, hash.Hash, superFilter, bloomBucketService.GetBloomFilterBucket().ToList());
                Assert.True(result);
            }
        }


        [Test]
        public void AddToDatabaseTest()
        {


            var str = "CountrycodeUci";
            //Enum.TryParse("Active", out myHashType);
            var a = (int)(Enum.TryParse(str.ToUpper(), true, out HashTypeEnum myHashType) ? myHashType : 0);




            //Assert.AreEqual(_coronapassContext.RevocationBatch.Count(), 102);
            //Assert.AreEqual(_coronapassContext.RevocationHash.Count(), 1638);
            //Assert.AreEqual(_coronapassContext.RevocationSuperFilter.Count(), 12);

        }


    }

}
