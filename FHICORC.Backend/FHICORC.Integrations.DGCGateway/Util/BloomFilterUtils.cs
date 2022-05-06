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


            var capacity = BestM(expectedElements, errorRate);
            var hashes = BestK(expectedElements, capacity);


            return new BloomStats()
            {
                ExpectedElements = expectedElements,
                ErrorRate = errorRate,
                m = capacity,
                k = hashes
            };
        }

        public static BloomStats CalcOptimalNP(int m, int k)
        {
            if (m < 1)
            {
                throw new ArgumentOutOfRangeException("capacity", m, "capacity must be > 0");
            }

            if (k < 1)
            {
                throw new ArgumentOutOfRangeException("hashes", k, "hashes must be > 0");
            }

            var expectedElements = BestN(k, m);
            var errorRate = BestP(k, m, expectedElements);

            return new BloomStats()
            {
                ExpectedElements = expectedElements,
                ErrorRate = errorRate,
                m = m,
                k = k
            };
        }

        /// <summary>
        /// Calculates the optimal size of the bloom filter in bits given expectedElements
        /// (expected number of elements in bloom filter) and falsePositiveProbability (tolerable
        /// false positive rate).
        /// </summary>
        /// <param name="n">Expected number of elements inserted in the bloom filter</param>
        /// <param name="p">Tolerable false positive rate.</param>
        /// <returns>the optimal size of the bloom filter in bits</returns>
        public static int BestM(long n, double p)
        {
            return (int)Math.Ceiling(-1.0 * ((double)n * Math.Log(p)) / Math.Pow(Math.Log(2.0), 2.0));
        }


        /// <summary>
        /// Calculates the optimal hashes(number of hash function) given expectedElements
        /// (expected number of elements in bloom filter) and size(size of bloom filter
        /// in bits).
        /// </summary>
        /// <param name="n">Expected number of elements inserted in the bloom filter</param>
        /// <param name="m">The size of the bloom filter in bits.</param>
        /// <returns>the optimal amount of hash functions hashes</returns>
        public static int BestK(long n, long m)
        {
            return (int)Math.Ceiling(Math.Log(2.0) * (double)m / (double)n);
        }

        /// <summary>
        /// Calculates the amount of elements a Bloom filter for which the given configuration
        /// of size and hashes is optimal.
        /// </summary>
        /// <param name="k">number of hashes</param>
        /// <param name="m">The size of the bloom filter in bits.</param>
        /// <returns>mount of elements a Bloom filter for which the given configuration of size and 
        /// hashes is optimal</returns>
        public static int BestN(long k, long m)
        {
            return (int)Math.Ceiling(Math.Log(2.0) * (double)m / (double)k);
        }

        /// <summary>
        /// Calculates the best-case (uniform hash function) false positive probability.
        /// </summary>
        /// <param name="k">number of hashes</param>
        /// <param name="m">The size of the bloom filter in bits.</param>
        /// <returns>The calculated false positive probability</returns>
        public static double BestP(long k, long m, double insertedElements)
        {
            return Math.Pow(1.0 - Math.Exp((double)(-k) * insertedElements / (double)m), k);
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
