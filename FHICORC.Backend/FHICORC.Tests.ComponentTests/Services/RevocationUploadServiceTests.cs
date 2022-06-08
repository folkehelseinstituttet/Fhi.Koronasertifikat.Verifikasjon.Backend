using System.Collections.Generic;
using System.Linq;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Services;
using FHICORC.Infrastructure.Database.Context;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace FHICORC.Tests.ComponentTests.Services
{
    [Category("Unit")]
    public class RevocationUploadServiceTests
    {
        ILogger<RevocationUploadService> loggerRevocationUploadService = new NullLoggerFactory().CreateLogger<RevocationUploadService>();
        private CoronapassContext _coronapassContext;
        private RevocationUploadService _revocationUploadService;

        [SetUp]
        public void Setup()
        {
            _coronapassContext = SeedDb.GetInMemoryContext();
            _revocationUploadService = new RevocationUploadService(loggerRevocationUploadService, _coronapassContext, FillBatchOptions());
        }

        private List<string> GetRandomHashesInLists(int newHashAmount)
        {
            var hashList = new List<string>();

            for (int i = 0; i < newHashAmount; i++)
            {
                hashList.Add(i.ToString());
            }
            return hashList;
        }

        [TestCase(999, 1), Order(1)]
        public void UploadHashesTestAddOneBatch(int newHashAmount, int addedBatches)
        {
            // Arrange
            var list = GetRandomHashesInLists(newHashAmount);
            var list2 = new List<string> { "abc" };
            var batchCountBefore = _coronapassContext.RevocationBatch.Count();
            var hashCountBefore = _coronapassContext.RevocationHash.Count();

            // Act
            _revocationUploadService.UploadHashes(list);
            var batchCountAfterFirstAct = _coronapassContext.RevocationBatch.Count();
            var hashCountAfterFirstAct = _coronapassContext.RevocationHash.Count();
            _revocationUploadService.UploadHashes(list2);
            var batchCountAfterSecondAct = _coronapassContext.RevocationBatch.Count();
            var hashCountAfterSecondAct = _coronapassContext.RevocationHash.Count();

            // Assert
            Assert.AreEqual(batchCountBefore + addedBatches, batchCountAfterFirstAct);
            Assert.AreEqual(hashCountBefore + newHashAmount, hashCountAfterFirstAct);
            Assert.True(_coronapassContext.RevocationHash.Any(x => x.Hash.Equals("abc")));
            Assert.AreEqual(batchCountAfterFirstAct, batchCountAfterSecondAct);
            Assert.AreEqual(hashCountAfterFirstAct + 1, hashCountAfterSecondAct);

        }

        [TestCase(1500, 2), Order(2)]
        public void UploadHashesTestAddTwoBatch(int newHashAmount, int addedBatches)
        {
            // Arrange
            var list = GetRandomHashesInLists(newHashAmount);
            var batchCountBefore = _coronapassContext.RevocationBatch.Count();
            var hashCountBefore = _coronapassContext.RevocationHash.Count();

            // Act
            _revocationUploadService.UploadHashes(list);
            var batchCountAfterAct = _coronapassContext.RevocationBatch.Count();
            var hashCountAfterAct = _coronapassContext.RevocationHash.Count();

            // Assert
            Assert.AreEqual(batchCountBefore + addedBatches, batchCountAfterAct);
            Assert.AreEqual(hashCountBefore + newHashAmount, hashCountAfterAct);
        }

        [TestCase(1, 1), Order(3)]
        public void UploadHashesTestAddOneHashAndBatch(int newHashAmount, int addedBatches)
        {
            // Arrange
            var list = GetRandomHashesInLists(newHashAmount);
            var batchCountBefore = _coronapassContext.RevocationBatch.Count();
            var hashCountBefore = _coronapassContext.RevocationHash.Count();

            // Act
            _revocationUploadService.UploadHashes(list);
            var batchCountAfterAct = _coronapassContext.RevocationBatch.Count();
            var hashCountAfterAct = _coronapassContext.RevocationHash.Count();

            // Assert
            Assert.AreEqual(batchCountBefore + addedBatches, batchCountAfterAct);
            Assert.AreEqual(hashCountBefore + newHashAmount, hashCountAfterAct);
        }

        [TearDown]
        public void TearDown()
        {
            _coronapassContext.Database.EnsureDeleted();
        }

        public BatchOptions FillBatchOptions()
        {
            return new BatchOptions()
            {
                BatchSize = 1000,
                CountryCode = "NO",
                HashType = "COUNTRYCODEUCI"
            };
        }
    }
}