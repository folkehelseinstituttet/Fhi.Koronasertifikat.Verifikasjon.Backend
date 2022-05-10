using System.Linq;
using NUnit.Framework;

namespace FHICORC.Tests.ComponentTests.DGCGComponentTests
{
    [Category("Component")]
    public class MobileDemoTests : SeedDb
    {
      

        [Test]
        public void IsSingleHashInSuperFilter() {
            var result = MobileUtil.ContainsCertificateFilterMobile("RO", "p8dtvN0/9YKtwgGAxvsvBg==", _coronapassContext, bloomBucketService.GetBloomFilterBucket());

            Assert.True(result);
        }

        [Test]
        public void AreAllHashesInSuperFilter()
        {
            foreach (var hash in _coronapassContext.RevocationHash) {
                var country = _coronapassContext.RevocationBatch.Find(hash.BatchId).Country;
                var result = MobileUtil.ContainsCertificateFilterMobile(country, hash.Hash, _coronapassContext, bloomBucketService.GetBloomFilterBucket());
                Assert.True(result);
            }
        }


        [Test]
        public void AddToDatabaseTest()
        {
            Assert.AreEqual(_coronapassContext.RevocationBatch.Count(), 102);
            Assert.AreEqual(_coronapassContext.RevocationHash.Count(), 1638);
            Assert.AreEqual(_coronapassContext.RevocationSuperFilter.Count(), 12);

        }


    }

}
