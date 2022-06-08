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
        private readonly IEnumerable<BucketItem> bloomFilterBuckets;

        public BloomBucketService(ILogger<BloomBucketService> logger, BloomBucketOptions bloomBucketOptions)
        {
            _logger = logger;
            _bloomBucketOptions = bloomBucketOptions;

            bloomFilterBuckets = CalculateBloomFilterBuckets();
        }


        public IEnumerable<BucketItem> CalculateBloomFilterBuckets() {

            //var _l = new List<int>() { 5, 10, 100, 250, 500, 1000 };
            var bloomFilterBucketsList = new List<BucketItem>();

            for (var i = 0; i < _bloomBucketOptions.NumberOfBuckets; i++) {
                var bucketValue = (int)Math.Ceiling((_bloomBucketOptions.MaxValue - _bloomBucketOptions.MinValue) / 
                    (Math.Pow(_bloomBucketOptions.NumberOfBuckets - 1, _bloomBucketOptions.Stepness)) 
                    * Math.Pow(i, _bloomBucketOptions.Stepness) + _bloomBucketOptions.MinValue);

                var bloomStats = BloomFilterUtils.CalcOptimalMK(bucketValue, _bloomBucketOptions.FalsePositiveProbability);
                var bucketItem = new BucketItem(i, bucketValue, bloomStats.m, bloomStats.k);
                bloomFilterBucketsList.Add(bucketItem);

            }

            return bloomFilterBucketsList;

        }


        public IEnumerable<BucketItem> GetBloomFilterBucket() => bloomFilterBuckets;

        public BucketItem GetBucketItemByBatchCount(int superBatchCount) {
            return bloomFilterBuckets.Where(b => superBatchCount <= b.MaxValue ).FirstOrDefault();
        }

        public int GetBucketIdx(int superBatchCount) { 
            return bloomFilterBuckets.Where(b => superBatchCount <= b.MaxValue).FirstOrDefault().BucketId;

        }
    }
}
