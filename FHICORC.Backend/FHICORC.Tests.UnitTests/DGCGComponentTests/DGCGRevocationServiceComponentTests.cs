using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;
using BloomFilter;
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
        private IBloomBucketService bloomBucketService;

        [SetUp]
        public void Setup()
        {
            var bloomBucketOptions = FillInBucketOptions();
            ILogger<BloomBucketService> loggerBloomBucketService = new NullLoggerFactory().CreateLogger<BloomBucketService>();
            bloomBucketService = new BloomBucketService(loggerBloomBucketService, bloomBucketOptions);

            _coronapassContext = SeedDb.GetInMemoryContext();
            _dgcgRevocationService = new DGCGRevocationService(loggerDGCGRevocationService, _coronapassContext, _dgcgService, bloomBucketOptions, bloomBucketService);
            SeedDatabase();
        }

        public BloomBucketOptions FillInBucketOptions() {
            return new BloomBucketOptions()
            {
                ExpieryDateLeewayInDays = 100,
                FalsePositiveProbability = 1e-10,
                MaxValue = 1000,
                MinValue = 5,
                NumberOfBuckets = 10,
                Stepness =  2.5
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


        [Test]
        public void IsSingleHashInSuperFilter() {
            var result = MobileFilter.ContainsCertificateFilterMobile("RO", "p8dtvN0/9YKtwgGAxvsvBg==", _coronapassContext, bloomBucketService.GetBloomFilterBucket());

            Assert.True(result);
        }

        [Test]
        public void AreAllHashesInSuperFilter()
        {
            foreach (var hash in _coronapassContext.RevocationHash) {
                var country = _coronapassContext.RevocationBatch.Find(hash.BatchId).Country;
                var result = MobileFilter.ContainsCertificateFilterMobile(country, hash.Hash, _coronapassContext, bloomBucketService.GetBloomFilterBucket());
                Assert.True(result);
            }
        }


        [Test]
        public void AddToDatabaseTest()
        {

            Assert.AreEqual(_coronapassContext.RevocationBatch.Count(), 102);
            Assert.AreEqual(_coronapassContext.RevocationHash.Count(), 1638);
            Assert.AreEqual(_coronapassContext.RevocationSuperFilter.Count(), 12);

        }

        [TearDown]
        public void TearDown()
        {
            _coronapassContext.Database.EnsureDeleted();
        }


        public class Demo
        {
            static IBloomFilter bf = FilterBuilder.Build(10000000, 0.01, HashFunction.Functions[HashMethod.Murmur3KirschMitzenmacher]);

            public void Sample()
            {
                bf.Add("Value");
                Console.WriteLine(bf.Contains("Value"));
            }
        }

    }

}
