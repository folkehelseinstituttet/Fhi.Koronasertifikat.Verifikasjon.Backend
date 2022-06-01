﻿using FHICORC.Application.Models;
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
    public class RevocationService : IRevocationService
    {
        private readonly ILogger<RevocationService> _logger;
        private readonly CoronapassContext _coronapassContext;
        private readonly IBloomBucketService _bloomBucketService;

        public RevocationService(ILogger<RevocationService> logger, CoronapassContext coronapassContext, IBloomBucketService bloomBucketService)
        {
            _logger = logger;
            _coronapassContext = coronapassContext;
            _bloomBucketService = bloomBucketService;
        }

        public bool ContainsCertificate(string dcc, string country) {
            return BloomFilterUtils.IsHashRevocated(dcc, country, _coronapassContext, _bloomBucketService.GetBloomFilterBucket());
        }

        public List<SuperBatch> FetchSuperBatches(DateTime dateTime)
        {
            try
            {
                var superBatchList = _coronapassContext.RevocationSuperFilter
                    .Where(s => s.Modified >= dateTime)
                    .Select(x => new SuperBatch()
                    {
                        Id = x.Id,
                        CountryISO3166 = x.SuperCountry,
                        BucketType = x.Bucket,
                        BloomFilter = x.SuperFilter,
                        HashType = (HashTypeEnum)x.HashType,
                        ExpirationDate = x.SuperExpires
                    }
                    ).ToList();

                if (!superBatchList.Any())
                    return null;

                return superBatchList;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public BloomFilterBuckets FetchBucketInfo() {
            return _bloomBucketService.GetBloomFilterBucket();
        }
    }
}
