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

namespace FHICORC.Tests.UnitTests.DGCGComponentTests
{
    [Category("Unit")]
    public class DGCGRevocationServiceComponentTests
    {
        ILogger<DgcgResponseParser> nullLogger = new NullLoggerFactory().CreateLogger<DgcgResponseParser>();
        private readonly Mock<FeatureToggles> featureTogglesMock = new Mock<FeatureToggles>();
        private CoronapassContext _coronapassContext;

        private DgcgRevocationListBatchItem batchRoot;
        private DGCGRevocationBatchRespondDto batch;

        [SetUp]
        public void Setup()
        {
         
        }


        [Test]
        public void Download() {


            var parsedResponse = JsonConvert.DeserializeObject<DgcgRevocationBatchListRespondDto>(File.ReadAllText("TestFiles/tst_revocation_batch_list.json"));
            _coronapassContext = SeedDb.GetInMemoryContext();


        }

        


    }
}