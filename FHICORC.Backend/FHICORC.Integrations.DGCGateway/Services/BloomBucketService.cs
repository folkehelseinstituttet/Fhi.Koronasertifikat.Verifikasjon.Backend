using Microsoft.Extensions.Logging;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using FHICORC.Integrations.DGCGateway.Util;
using System.Collections.Generic;
using System.Linq;
using System;

namespace FHICORC.Integrations.DGCGateway.Services
{
    public class BloomBucketService : IBloomBucketService
    {
        private readonly ILogger<BloomBucketService> _logger;
        private readonly BloomBucketOptions _bloomBucketOptions;
        private readonly BloomFilterBuckets bloomFilterBuckets;

        public BloomBucketService(ILogger<BloomBucketService> logger, BloomBucketOptions bloomBucketOptions)
        {
            _logger = logger;
            _bloomBucketOptions = bloomBucketOptions;

            bloomFilterBuckets = CalculateBloomFilterBuckets();
        }


        public BloomFilterBuckets CalculateBloomFilterBuckets() {

            //var _l = new List<int>() { 5, 10, 100, 250, 500, 1000 };
            var bloomFilterBucketsList = new List<BucketItem>();

            for (var i = 0; i < _bloomBucketOptions.NumberOfBuckets; i++) {
                var bucketValue = (int)Math.Ceiling((_bloomBucketOptions.MaxValue - _bloomBucketOptions.MinValue) / 
                    (Math.Pow(_bloomBucketOptions.NumberOfBuckets - 1, _bloomBucketOptions.Stepness)) 
                    * Math.Pow(i, _bloomBucketOptions.Stepness) + _bloomBucketOptions.MinValue);

                var bloomStats = BloomFilterUtils.CalcOptimalMK(bucketValue, _bloomBucketOptions.FalsePositiveProbability);
                var bucketItem = new BucketItem()
                {
                    BucketId = i,
                    MaxValue = bucketValue,
                    BitVectorLength_m = bloomStats.m,
                    NumberOfHashFunctions_k = bloomStats.k,
                };
                bloomFilterBucketsList.Add(bucketItem);

            }

            return new BloomFilterBuckets() { Buckets = bloomFilterBucketsList };

        }


        public BloomFilterBuckets GetBloomFilterBucket() {
            return bloomFilterBuckets;
        }

        public BucketItem GetBucketItemByBatchCount(int superBatchCount) {
            return bloomFilterBuckets.Buckets.Where(b => superBatchCount <= b.MaxValue ).FirstOrDefault();
        }

        public int GetBucketIdx(int superBatchCount) { 
            return bloomFilterBuckets.Buckets.Where(b => superBatchCount <= b.MaxValue).FirstOrDefault().BucketId;

        }
    }
}
