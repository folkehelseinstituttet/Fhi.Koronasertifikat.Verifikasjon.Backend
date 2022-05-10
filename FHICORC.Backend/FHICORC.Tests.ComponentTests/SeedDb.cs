using System;
using FHICORC.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Cryptography.Pkcs;
using System.Text;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Services;
using FHICORC.Integrations.DGCGateway.Services;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;

namespace FHICORC.Tests.ComponentTests
{
    public class SeedDb
    {
        protected ILogger<DGCGRevocationService> loggerDGCGRevocationService = new NullLoggerFactory().CreateLogger<DGCGRevocationService>();
        protected ILogger<RevocationService> loggerRevocationService = new NullLoggerFactory().CreateLogger<RevocationService>();
        protected CoronapassContext _coronapassContext;
        protected DGCGRevocationService _dgcgRevocationService;
        protected RevocationService _revocationService;
        protected readonly IDgcgService _dgcgService = Substitute.For<IDgcgService>();
        protected IBloomBucketService bloomBucketService;

        [SetUp]
        public void Setup()
        {
            var bloomBucketOptions = FillInBucketOptions();
            ILogger<BloomBucketService> loggerBloomBucketService = new NullLoggerFactory().CreateLogger<BloomBucketService>();
            bloomBucketService = new BloomBucketService(loggerBloomBucketService, bloomBucketOptions);

            _coronapassContext = SeedDb.GetInMemoryContext();
            _dgcgRevocationService = new DGCGRevocationService(loggerDGCGRevocationService, _coronapassContext, _dgcgService, bloomBucketOptions, bloomBucketService);

            _revocationService = new RevocationService(loggerRevocationService, _coronapassContext, bloomBucketService);
            SeedDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            _coronapassContext.Database.EnsureDeleted();
        }

        public BloomBucketOptions FillInBucketOptions()
        {
            return new BloomBucketOptions()
            {
                ExpieryDateLeewayInDays = 100,
                FalsePositiveProbability = 1e-10,
                MaxValue = 1000,
                MinValue = 5,
                NumberOfBuckets = 10,
                Stepness = 2.5
            };
        }


        public void SeedDatabase()
        {

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

            _dgcgRevocationService.OrganizeBatches();

        }




        public static CoronapassContext GetInMemoryContext()
        {
            var dbNavn = TestContext.CurrentContext.Test.Name;
            var options = new DbContextOptionsBuilder<CoronapassContext>()
                .UseInMemoryDatabase(databaseName: dbNavn)
                .Options;

            return new CoronapassContext(options);
        }

    }
}
