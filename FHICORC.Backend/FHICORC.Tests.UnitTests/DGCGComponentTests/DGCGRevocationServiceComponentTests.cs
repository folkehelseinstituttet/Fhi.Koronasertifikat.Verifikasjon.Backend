using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Services;
using FHICORC.Infrastructure.Database.Context;
using FHICORC.Integrations.DGCGateway.Services;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using FHICORC.Integrations.DGCGateway.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;

namespace FHICORC.Tests.UnitTests.DGCGComponentTests
{
    [Category("Unit")]
    public class DGCGRevocationServiceComponentTests
    {
        ILogger<DGCGRevocationService> loggerDGCGRevocationService = new NullLoggerFactory().CreateLogger<DGCGRevocationService>();
        ILogger<RevocationService> loggerRevocationService = new NullLoggerFactory().CreateLogger<RevocationService>();
        private CoronapassContext _coronapassContext;
        private DGCGRevocationService _dgcgRevocationService;
        private readonly IDgcgService _dgcgService = Substitute.For<IDgcgService>();
        private RevocationService _revocationService;

        [SetUp]
        public void Setup()
        {
            _coronapassContext = SeedDb.GetInMemoryContext();
            _dgcgRevocationService = new DGCGRevocationService(loggerDGCGRevocationService, _coronapassContext, _dgcgService);
            _revocationService = new RevocationService(loggerRevocationService, _coronapassContext, FillBatchOptions());
            SeedDatabase();
    }

        public void SeedDatabase() {

            var revocationBatchList = JsonConvert.DeserializeObject<DgcgRevocationBatchListRespondDto>(File.ReadAllText("TestFiles/tst_revocation_batch_list.json"));

            foreach (var rb in revocationBatchList.Batches)
            {
                var response = File.ReadAllText("TestFiles/BatchHashes/" + rb.BatchId + ".txt");

                try
                {
                    var encodedMessage = Convert.FromBase64String(response);

                    var signedCms = new SignedCms();
                    signedCms.Decode(encodedMessage);
                    signedCms.CheckSignature(true);

                    var decodedMessage = Encoding.UTF8.GetString(signedCms.ContentInfo.Content);
                    var parsedResponse = JsonConvert.DeserializeObject<DGCGRevocationBatchRespondDto>(decodedMessage);

                    _dgcgRevocationService.AddToDatabase(rb, parsedResponse);


                }
                catch (Exception e) { }
            }
        }

        private (List<string> HashCollectionOne, List<string> HashCollectionTwo) GetRandomHashesInLists(int newHashAmount)
        {
            var hashList = new List<string>();
            var hashList2 = new List<string>();
            for (int i = 0; i < newHashAmount; i++)
            {
                hashList.Add(i.ToString());
                hashList2.Add((i+10000).ToString());
            }
            return (hashList, hashList2);
        }

        [TestCase (2999, 3)]
        public void UploadHashesTest(int newHashAmount, int addedBatches)
        {
            // Arrange
            (var list1, var list2) = GetRandomHashesInLists(newHashAmount);
            var batchCountBefore = _coronapassContext.RevocationBatch.Count();
            var hashCountBefore = _coronapassContext.RevocationHash.Count();

            // Act
            _revocationService.UploadHashes(list1);
            var batchCountAfterFirstUpload = _coronapassContext.RevocationBatch.Count();
            var hashCountAfterFirstUpload = _coronapassContext.RevocationHash.Count();

            // Assert
            Assert.AreEqual(batchCountBefore + addedBatches, batchCountAfterFirstUpload);
            Assert.AreEqual(hashCountBefore + newHashAmount, hashCountAfterFirstUpload );

            // Arrange
            list1.Add("abc");

            // Act
            _revocationService.UploadHashes(list1);
            var batchCountAfterSecond = _coronapassContext.RevocationBatch.Count();
            var hashCountAfterSecond = _coronapassContext.RevocationHash.Count();

            // Assert
            Assert.True(_coronapassContext.RevocationHash.Where(x => x.Hash.Equals("abc")).FirstOrDefault().Hash.Equals("abc"));
            Assert.AreEqual(batchCountAfterFirstUpload, batchCountAfterSecond);
            Assert.AreEqual(hashCountAfterFirstUpload + newHashAmount + 1, hashCountAfterSecond);

            // Arrange
            var countBatchesAfter2 = _coronapassContext.RevocationBatch.Count();
            var countHashesAfter2 = _coronapassContext.RevocationHash.Count();
            list1.Add("abc");

            // Act 
            _revocationService.UploadHashes(list1);

            // Assert
            Assert.AreEqual(_coronapassContext.RevocationBatch.Count(), countBatchesAfter2);
            Assert.AreEqual(_coronapassContext.RevocationHash.Count(), countHashesAfter2);

            // Arrange
            var countBatchesAfter3 = _coronapassContext.RevocationBatch.Count();
            var countHashesAfter3 = _coronapassContext.RevocationHash.Count();

            // Act
            _revocationService.UploadHashes(list2);

            // Assert
            Assert.AreEqual(_coronapassContext.RevocationBatch.Count(), countBatchesAfter3 + addedBatches);
            Assert.AreEqual(_coronapassContext.RevocationHash.Count(), countHashesAfter3 + list2.Count);
        }

        [Test]
        public void AddToDatabaseTest() {

            Assert.AreEqual(_coronapassContext.RevocationBatch.Count(), 102);
            Assert.AreEqual(_coronapassContext.RevocationFilter.Count(), 102);
            Assert.AreEqual(_coronapassContext.RevocationHash.Count(), 1638);
            Assert.AreEqual(_coronapassContext.RevocationSuperFilter.Count(), 2);
        }


        [Test]
        public void SuperFilterTest()
        {
            _coronapassContext.RevocationHash
                .ToList()
                .ForEach(h => Assert.True(_revocationService.ContainsCertificate(h.Hash)));
        }


        [Test]
        public void DeleteExpiredBatchesTest() {

            //Assume
            var batchId = "699978cf-d2d4-4093-8b54-ab2cf695d76d";
            var expiredBatch = _coronapassContext.RevocationBatch.Find(batchId);
            expiredBatch.Expires = DateTime.UtcNow.AddDays(-100);
            _coronapassContext.SaveChanges();

            var hashInExpiredBatch = expiredBatch.RevocationHashes.FirstOrDefault().Hash;
            var inSuperFilterBefore = _revocationService.ContainsCertificateFilter(hashInExpiredBatch);


            //Check if superfilter contains all hashes
            SuperFilterTest();


            //Act 
            _dgcgRevocationService.DeleteExpiredBatches();


            //Assert
            Assert.True(expiredBatch.Deleted);
            Assert.IsNull(expiredBatch.SuperId);


            // Check if hashes from deleted batcj are removed from superfilter
            _coronapassContext.RevocationHash
                .Where(x => x.BatchId == batchId)
                .ToList()
                .ForEach(h => Assert.False(_revocationService.ContainsCertificateFilter(h.Hash)));

            // Check if ALL the other hashes are still in the superfilter
            //_coronapassContext.RevocationHash
            //    .Where(x => x.BatchId != batchId)
            //    .ToList()
            //    .ForEach(h => Assert.True(revocationService.ContainsCertificateFilter(h.Hash)));
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