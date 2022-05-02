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
using FHICORC.Integrations.DGCGateway.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
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

        [SetUp]
        public void Setup()
        {
            _coronapassContext = SeedDb.GetInMemoryContext();
            _dgcgRevocationService = new DGCGRevocationService(loggerDGCGRevocationService, _coronapassContext);
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
                catch (Exception e)
                {

                }

            }
        }


        [Test]
        public void CheckIfSuperFilterWork()
        {
            var revocationService = new RevocationService(loggerRevocationService, _coronapassContext);

            _coronapassContext.HashesRevoc
                .ToList()
                .ForEach(h => Assert.True(revocationService.ContainsCertificate(h.Hash)));
        }



        [Test]
        public void DeleteExpiredBatchesTest() {

            //Assume
            var batchId = "699978cf-d2d4-4093-8b54-ab2cf695d76d";
            var expiredBatch = _coronapassContext.BatchesRevoc.Find(batchId);
            expiredBatch.Expires = DateTime.UtcNow.AddDays(-100);
            _coronapassContext.SaveChanges();

            var revocationService = new RevocationService(loggerRevocationService, _coronapassContext);
            var hashInExpiredBatch = expiredBatch.HashesRevocs.FirstOrDefault().Hash;
            var inSuperFilterBefore = revocationService.ContainsCertificateFilter(hashInExpiredBatch);

            CheckIfSuperFilterWork();


            //Act 
            _dgcgRevocationService.DeleteExpiredBatches();


            //Assert
            Assert.True(expiredBatch.Deleted);
            Assert.IsNull(expiredBatch.SuperId);

            _coronapassContext.HashesRevoc
                .Where(x => x.BatchId == batchId)
                .ToList()
                .ForEach(h => Assert.False(revocationService.ContainsCertificateFilter(h.Hash)));

            _coronapassContext.HashesRevoc
                .Where(x => x.BatchId != batchId)
                .ToList()
                .ForEach(h => Assert.True(revocationService.ContainsCertificateFilter(h.Hash)));

        }


        [TearDown]
        public void TearDown()
        {
            _coronapassContext.Database.EnsureDeleted();
        }





    }
}