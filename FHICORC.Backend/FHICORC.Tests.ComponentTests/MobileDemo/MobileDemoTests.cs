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
      

        //[TestCase("RO", "J4PCK4sFs63kH/EeP7+C3A==")]

        //[TestCase("YWFhYWFhYWFhYWFhYWFhYQ==")]
        //[TestCase("p8dtvN0/9YKtwgGAxvsvBg==")]
        [TestCase("CZ", "+hkApE6qvfhfb95y/Jjx/w==")]
        public void IsSingleHashInSuperFilter(string countryIso, string str) {
            var superFilter = _coronapassContext.RevocationSuperFilter.Where(x => x.SuperCountry == countryIso).ToList();
            var result = MobileUtil.ContainsCertificateFilterMobile(str, str, superFilter, bloomBucketService.GetBloomFilterBucket().ToList());

            Assert.True(result);
        }

        [Test]
        public void AreAllHashesInSuperFilter()
        {
            var superFilter = _coronapassContext.RevocationSuperFilter.ToList();
            foreach (var hash in _coronapassContext.RevocationHash) {                
                var result = MobileUtil.ContainsCertificateFilterMobile(hash.Hash, hash.Hash, superFilter, bloomBucketService.GetBloomFilterBucket().ToList());
                Assert.True(result);
            }
        }

    }

}
