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
        public void AddToDatabaseTest()
        {
            Assert.AreEqual(_coronapassContext.RevocationBatch.Count(), 102);
            Assert.AreEqual(_coronapassContext.RevocationHash.Count(), 1638);
            Assert.AreEqual(_coronapassContext.RevocationSuperFilter.Count(), 12);

        }

    }

}
