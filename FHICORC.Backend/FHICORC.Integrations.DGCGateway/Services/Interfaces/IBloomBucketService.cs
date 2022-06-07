using FHICORC.Application.Models;
using System.Collections.Generic;

namespace FHICORC.Integrations.DGCGateway.Services.Interfaces
{
    public interface IBloomBucketService
    {
        public IEnumerable<BucketItem> CalculateBloomFilterBuckets();
        public IEnumerable<BucketItem> GetBloomFilterBucket();
        public BucketItem GetBucketItemByBatchCount(int superBatchCount);
        public int GetBucketIdx(int superBatchCount);
    }

}
