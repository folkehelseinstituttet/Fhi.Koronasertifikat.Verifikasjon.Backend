namespace FHICORC.Application.Models
{
    public class BucketItem
    {

        public BucketItem(int bucketId, int maxValue, int bitVectorLength_m, int numberOfHashFunctions_k)
        {
            this.BucketId = bucketId;
            this.MaxValue = maxValue;
            this.BitVectorLength_m = bitVectorLength_m;
            this.NumberOfHashFunctions_k = numberOfHashFunctions_k;
        }


        public int BucketId { get; private set; }
        public int MaxValue { get; private set; }
        public int BitVectorLength_m { get; private set; }
        public int NumberOfHashFunctions_k { get; private set; }



    }
}