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



    }
}