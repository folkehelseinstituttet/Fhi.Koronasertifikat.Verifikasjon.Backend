using FHICORC.Application.Models;

namespace FHICORC.Integrations.DGCGateway.Services.Interfaces
{
    public interface IBloomBucketService
    {
        public BloomFilterBuckets CalculateBloomFilterBuckets();
        public BloomFilterBuckets GetBloomFilterBucket();
        public BucketItem GetBucketItemByBatchCount(int superBatchCount);
        public int GetBucketIdx(int superBatchCount);
    }

}
