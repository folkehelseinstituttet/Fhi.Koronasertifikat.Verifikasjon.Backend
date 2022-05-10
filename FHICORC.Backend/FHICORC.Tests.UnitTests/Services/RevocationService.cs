using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHICORC.Tests.UnitTests.Services
{
    [Category("Unit")]
    public class RevocationService : SeedDb
    {

        [Test]
        public void IsSingleHashInSuperFilter()
        {
            var result = _revocationService.ContainsCertificate("p8dtvN0/9YKtwgGAxvsvBg==", "RO");
            Assert.True(result);
        }

        [Test]
        public void AreAllHashesInSuperFilter()
        {
            foreach (var hash in _coronapassContext.RevocationHash)
            {
                var country = _coronapassContext.RevocationBatch.Find(hash.BatchId).Country;
                var result = _revocationService.ContainsCertificate(hash.Hash, country);
             
                Assert.True(result);
            }
        }



    }
}
