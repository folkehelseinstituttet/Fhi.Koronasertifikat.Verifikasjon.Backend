using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Domain.Models;
using FHICORC.Infrastructure.Database.Context;
using FHICORC.Integrations.DGCGateway.Models;
using FHICORC.Integrations.DGCGateway.Services;
using FHICORC.Integrations.DGCGateway.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace FHICORC.Tests.UnitTests.DGCGTests
{
    [Category("Unit")]
    public class DGCGRevocationServiceTests
    {
        ILogger<DgcgResponseParser> nullLogger = new NullLoggerFactory().CreateLogger<DgcgResponseParser>();
        private readonly Mock<FeatureToggles> featureTogglesMock = new Mock<FeatureToggles>();
        private CoronapassContext _coronapassContext;

        private DgcgRevocationListBatchItem batchRoot;
        private DGCGRevocationBatchRespondDto batch;

        [SetUp]
        public void Setup()
        {
            batchRoot = new DgcgRevocationListBatchItem()
            {
                BatchId = "abc",
                Country = "NO",
                Date = DateTime.UtcNow,
                Deleted = false
            };

            batch = new DGCGRevocationBatchRespondDto()
            {
                Country = "NO",
                HashType = "UCI",
                Expires = DateTime.UtcNow.AddDays(100),
                Kid = "dasido",
                Entries = new List<DgcgHashItem>() {
                    new DgcgHashItem(){ Hash = "hash1"},
                    new DgcgHashItem(){ Hash = "hash2"},
                    new DgcgHashItem(){ Hash = "hash3"}
                }
            };
        }

        [Test]
        public void FillInBatchRevocTest()
        {
            var revocationBatch = DGCGRevocationService.FillInBatchRevoc(batchRoot, batch);

            Assert.That(revocationBatch.BatchId.Equals(batchRoot.BatchId));
            Assert.That(revocationBatch.Country.Equals(batchRoot.Country));
            Assert.That(revocationBatch.Date.Equals(batchRoot.Date));
            Assert.That(revocationBatch.Deleted.Equals(batchRoot.Deleted));

            Assert.That(revocationBatch.HashType.Equals(batch.HashType));
            Assert.That(revocationBatch.Expires.Equals(batch.Expires));
            Assert.That(revocationBatch.Kid.Equals(batch.Kid));
            Assert.IsFalse(revocationBatch.Upload);
        }


        /// <summary>
        /// Tests if BloomFilters for a <c>Batch</c> are generated and contain the right information
        /// Checks that strings that are not encoded in the filter are rejected
        /// (This test has a <c>e-10</c> chance tho fail).
        /// </summary>
        [Test]
        public void GenerateBatchFilterTest() {
            //Assume
            var m = 47936;
            var k = 32;

            batch.Entries = new List<DgcgHashItem>();
            GenerateRandomStrings(1000).ForEach(s => batch.Entries.Add(new DgcgHashItem() { Hash = s }));

            //Act
            var filter = DGCGRevocationService.GenerateBatchFilter(batch.Entries, m, k);

            //Assert
            batch.Entries.ForEach(e => Assert.True(filter.Contains(e.Hash, m, k)));
            GenerateRandomStrings(1000).ForEach(s => Assert.False(filter.Contains(s, m, k)));
            Assert.False(filter.Contains("thisShouldNotExist", m, k));
        }

        private List<string> GenerateRandomStrings(int numberOfStrings)
        {
            var listOfStrings = new List<string>();

            for (var i = 0; i < numberOfStrings; i++)
            {
                var g = Guid.NewGuid();
                var GuidString = Convert.ToBase64String(g.ToByteArray());
                listOfStrings.Add(GuidString);
            }

            return listOfStrings;
        }


        


    }
}