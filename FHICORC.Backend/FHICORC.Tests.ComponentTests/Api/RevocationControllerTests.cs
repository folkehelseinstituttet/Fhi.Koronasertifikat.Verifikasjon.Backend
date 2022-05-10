using System;
using System.Linq;
using FHICORC.ApplicationHost.Api.Controllers;
using NUnit.Framework;

namespace FHICORC.Tests.ComponentTests.Api
{
    [Category("Component")]
    public class RevocationControllerTests : SeedDb
    {

        [Test]
        public void DownloadRevocationSuperBatchesTest() {
            var sut = new RevocationController(_revocationService);
            var result = sut.DownloadRevocationSuperBatches(DateTime.UtcNow);

            foreach (var superBatch in result.SuperBatches) {
                var revocationSuperFilter = _coronapassContext.RevocationSuperFilter.Find(superBatch.Id);
                //Assert.That(revocationSuperFilter.SuperFilter.Equals(superBatch.SuperFilter));
                //Assert.That(revocationSuperFilter.Bucket.Equals(superBatch.Bucket));
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
