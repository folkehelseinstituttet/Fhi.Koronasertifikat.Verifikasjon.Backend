using System.Text;
using System.Collections;
using System;
using BloomFilter;

namespace FHICORC.Integrations.DGCGateway.Util
{
    public static class BloomFilterUtils
    {
        public static BitArray AddToFilter(this BitArray filter, string str, int m, int k)
        {
            var hash = HashData(Encoding.UTF8.GetBytes(str), m, k);

            foreach (int i in hash)
                filter[i] = true;

            return filter;

        }

        public static bool Contains(this BitArray filter, string str, int m, int k)
        {
            var hash = HashData(Encoding.UTF8.GetBytes(str), m, k);

            foreach (int i in hash) {
                if (!filter[i])
                    return false;
            }
            return true;
        }

        public static bool Contains(this BitArray filter, int[] hashData)
        {
            foreach (int i in hashData)
            {
                var value = filter[i];
                if (!value)
                    return false;
            }
            return true;
        }

        public static int[] HashData(byte[] data, int m, int k)
        {
            var hashFunction = HashFunction.Functions[HashMethod.Murmur3KirschMitzenmacher];
            return hashFunction.ComputeHash(data, m, k);
        }

        public static byte[] BitToByteArray(BitArray bitArray) {

            byte[] byteArray = new byte[(bitArray.Length - 1) / 8 + 1];
            bitArray.CopyTo(byteArray, 0);

            return byteArray;

        }

        public static BloomStats CalcOptimalMK(int expectedElements, double errorRate)
        {
            if (expectedElements < 1)
            {
                throw new ArgumentOutOfRangeException("expectedElements", expectedElements, "expectedElements must be > 0");
            }

            if (errorRate >= 1.0 || errorRate <= 0.0)
            {
                throw new ArgumentOutOfRangeException("errorRate", errorRate, $"errorRate must be between 0 and 1, exclusive. Was {errorRate}");
            }

            var capacity = BloomFilter.Filter.BestM(expectedElements, errorRate);
            var hashes = BloomFilter.Filter.BestK(expectedElements, capacity);


            return new BloomStats()
            {
                ExpectedElements = expectedElements,
                ErrorRate = errorRate,
                m = capacity,
                k = hashes
            };
        }
    }

    public class BloomStats
    {
        public int ExpectedElements { get; set; }
        public double ErrorRate { get; set; }
        public int m { get; set; }
        public int k { get; set; }


    }
}
