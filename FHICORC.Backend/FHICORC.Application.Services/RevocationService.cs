using System.Text;
using System.Collections;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using FHICORC.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using FHICORC.Integrations.DGCGateway.Util;
using FHICORC.Application.Models;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using System.Collections.Generic;

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



        public SuperBatchesDto FetchSuperBatches(DateTime dateTime) {

            try
            {
                var superBatchList = _coronapassContext.RevocationSuperFilter
                    .Where(s => s.Modified <= dateTime)
                    .Select(x => new SuperBatch()
                    {
                        Id = x.Id,
                        Bucket = x.Bucket,
                        SuperFilter = x.SuperFilter,
                    }
                    ).ToList();

                    return new SuperBatchesDto()
                    {
                        SuperBatches = superBatchList
                    };
            }
            catch (Exception e) {
                return new SuperBatchesDto() {
                    SuperBatches = new List<SuperBatch>() { new SuperBatch() { Id = 42, Bucket = _coronapassContext.RevocationSuperFilter.Count() } } };   
                }
            
            }

        public BloomFilterBuckets FetchBucketInfo() {
            return _bloomBucketService.GetBloomFilterBucket();
        }
    }
}
