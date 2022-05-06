using System;
using System.Collections.Generic;

namespace FHICORC.Application.Models
{

    public class BloomFilterBuckets { 
        public List<BucketItem> Buckets { get; set; }
    }

    public class BucketItem
    {
        public int BucketId { get; set; }
        public int MaxValue { get; set; }
        public int BitVectorLength_m { get; set; }
        public int NumberOfHashFunctions_k { get; set; }

    }
}