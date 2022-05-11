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

        private (IEnumerable<string> HashCollectionOne, IEnumerable<string> HashCollectionTwo) GetRandomHashesInLists()
        {
            int amount = 2999;
            var hashList = new List<string>();
            var hashList2 = new List<string>();
            for (int i = 0; i < amount; i++)
            {
                hashList.Add(i.ToString());
                hashList2.Add((i+10000).ToString());
            }
            return (hashList, hashList2);
        }

        [Test]
        public void UploadHashesTest()
        {
            var hashLists = GetRandomHashesInLists();
            var list1 = hashLists.Item1.ToList();
            var list2 = hashLists.Item2.ToList();
            var countBatchesBefore = _coronapassContext.RevocationBatch.Count();
            var countHashesBefore = _coronapassContext.RevocationHash.Count();

            _revocationService.UploadHashes(list1);
            var countBatchesAfter = _coronapassContext.RevocationBatch.Count();
            var countHashesAfter = _coronapassContext.RevocationHash.Count();
            Assert.AreEqual(countBatchesAfter, countBatchesBefore + 3);
            Assert.AreEqual(countHashesAfter, countHashesBefore + 2999);

            list1.Add("abc");
            _revocationService.UploadHashes(list1);
            var countBatchesAfter2 = _coronapassContext.RevocationBatch.Count();
            var countHashesAfter2 = _coronapassContext.RevocationHash.Count();
            var hash = _coronapassContext.RevocationHash.Where(x => x.Hash.Equals("abc")).FirstOrDefault();
            Assert.True(hash.Hash.Equals("abc"));
            Assert.AreEqual(countBatchesAfter2, countBatchesAfter);
            Assert.AreEqual(countHashesAfter2, countHashesAfter + 1);

            list1.Add("abc");
            _revocationService.UploadHashes(list1);
            var countBatchesAfter3 = _coronapassContext.RevocationBatch.Count();
            var countHashesAfter3 = _coronapassContext.RevocationHash.Count();
            Assert.AreEqual(countBatchesAfter3, countBatchesAfter2);
            Assert.AreEqual(countHashesAfter3, countHashesAfter2);

            _revocationService.UploadHashes(list2);
            var countBatchesAfter4 = _coronapassContext.RevocationBatch.Count();
            var countHashesAfter4 = _coronapassContext.RevocationHash.Count();

            Assert.AreEqual(countBatchesAfter4, countBatchesAfter3 + 3);
            Assert.AreEqual(countHashesAfter4, countHashesAfter3 + list2.Count());
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