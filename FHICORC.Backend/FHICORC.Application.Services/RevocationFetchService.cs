using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Domain.Models;
using FHICORC.Infrastructure.Database.Context;
using FHICORC.Integrations.DGCGateway.Util;
using FHICORC.Application.Models;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using System.Collections.Generic;
using FHICORC.Core.Services.Enum;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace FHICORC.Application.Services
{
    public class RevocationFetchService : IRevocationFetchService
    {
        private readonly ILogger<RevocationFetchService> _logger;
        private readonly CoronapassContext _coronapassContext;
        private readonly IBloomBucketService _bloomBucketService;

        public RevocationFetchService(ILogger<RevocationFetchService> logger, CoronapassContext coronapassContext, IBloomBucketService bloomBucketService)
        {
            _logger = logger;
            _coronapassContext = coronapassContext;
            _bloomBucketService = bloomBucketService;
        }

        public bool ContainsCertificate(string dcc, string country) => BloomFilterUtils.IsHashRevocated(dcc, country, _coronapassContext, _bloomBucketService.GetBloomFilterBucket());

        public IEnumerable<SuperBatch> FetchSuperBatches(DateTime dateTime)
        {
            try
            {
                var superBatchList = _coronapassContext.RevocationSuperFilter
                    .Where(s => s.Modified >= dateTime)
                    .Select(x => new SuperBatch(x.Id, x.SuperCountry, x.Bucket, x.SuperFilter, (HashTypeEnum)x.HashType, x.SuperExpires));

                if (!superBatchList.Any())
                    return null;

                return superBatchList;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public IEnumerable<BucketItem> FetchBucketInfo() => _bloomBucketService.GetBloomFilterBucket();
    }
}
