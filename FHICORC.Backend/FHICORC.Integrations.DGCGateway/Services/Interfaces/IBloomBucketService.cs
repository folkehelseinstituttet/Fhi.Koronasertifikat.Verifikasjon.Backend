using FHICORC.Application.Models;
using System.Collections.Generic;

namespace FHICORC.Integrations.DGCGateway.Services.Interfaces
{
    public interface IBloomBucketService
    {
        public List<BucketItem> CalculateBloomFilterBuckets();
        public List<BucketItem> GetBloomFilterBucket();
        public BucketItem GetBucketItemByBatchCount(int superBatchCount);
        public int GetBucketIdx(int superBatchCount);
    }

}
