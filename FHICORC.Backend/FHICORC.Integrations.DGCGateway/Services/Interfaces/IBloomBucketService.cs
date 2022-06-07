using FHICORC.Application.Models;
using System.Collections.Generic;

namespace FHICORC.Integrations.DGCGateway.Services.Interfaces
{
    public interface IBloomBucketService
    {
        IEnumerable<BucketItem> CalculateBloomFilterBuckets();
        IEnumerable<BucketItem> GetBloomFilterBucket();
        BucketItem GetBucketItemByBatchCount(int superBatchCount);
        int GetBucketIdx(int superBatchCount);
    }

}
