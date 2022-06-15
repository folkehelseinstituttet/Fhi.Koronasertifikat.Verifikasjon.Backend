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
        protected ILogger<RevocationFetchService> loggerRevocationService = new NullLoggerFactory().CreateLogger<RevocationFetchService>();
        protected CoronapassContext _coronapassContext;
        protected DGCGRevocationService _dgcgRevocationService;
        protected RevocationFetchService _revocationService;
        protected readonly IDgcgService _dgcgService = Substitute.For<IDgcgService>();
        protected IBloomBucketService bloomBucketService;

        [SetUp]
        public void Setup()
        {
            var bloomBucketOptions = FillInBucketOptions();
            var featureToggles = new FeatureToggles() { SeedDbWithLocalData = true };
            ILogger<BloomBucketService> loggerBloomBucketService = new NullLoggerFactory().CreateLogger<BloomBucketService>();
            bloomBucketService = new BloomBucketService(loggerBloomBucketService, bloomBucketOptions);

            _coronapassContext = SeedDb.GetInMemoryContext();
            _dgcgRevocationService = new DGCGRevocationService(loggerDGCGRevocationService, _coronapassContext, _dgcgService, bloomBucketOptions, bloomBucketService, featureToggles);

            _revocationService = new RevocationFetchService(loggerRevocationService, _coronapassContext, bloomBucketService);
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
                NumberOfBuckets = 200,
                Stepness = 1
            };
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
