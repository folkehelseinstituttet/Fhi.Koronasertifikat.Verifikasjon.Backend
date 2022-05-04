using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using FHICORC.Application.Models;
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

        [SetUp]
        public void Setup()
        {
            _coronapassContext = SeedDb.GetInMemoryContext();
            _dgcgRevocationService = new DGCGRevocationService(loggerDGCGRevocationService, _coronapassContext, _dgcgService);
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
            var revocationService = new RevocationService(loggerRevocationService, _coronapassContext);
            _coronapassContext.RevocationHash
                .ToList()
                .ForEach(h => Assert.True(revocationService.ContainsCertificate(h.Hash)));
        }


        [Test]
        public void DeleteExpiredBatchesTest() {

            //Assume
            var batchId = "699978cf-d2d4-4093-8b54-ab2cf695d76d";
            var expiredBatch = _coronapassContext.RevocationBatch.Find(batchId);
            expiredBatch.Expires = DateTime.UtcNow.AddDays(-100);
            _coronapassContext.SaveChanges();

            var revocationService = new RevocationService(loggerRevocationService, _coronapassContext);
            var hashInExpiredBatch = expiredBatch.RevocationHashes.FirstOrDefault().Hash;
            var inSuperFilterBefore = revocationService.ContainsCertificateFilter(hashInExpiredBatch);


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
                .ForEach(h => Assert.False(revocationService.ContainsCertificateFilter(h.Hash)));

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

    }
}