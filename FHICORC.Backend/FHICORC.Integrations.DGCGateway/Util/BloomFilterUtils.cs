using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System;

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

            var hashAlgorithm = SHA256.Create();

            int[] array = new int[k];
            int num = 0;
            byte[] array2 = new byte[0];
            byte[] outputBuffer = new byte[hashAlgorithm.HashSize / 8];

            while (num < k)
            {
                hashAlgorithm.TransformBlock(array2, 0, array2.Length, outputBuffer, 0);
                array2 = hashAlgorithm.ComputeHash(data, 0, data.Length);
                BitArray bit = new BitArray(array2);
                int num2 = (int)(32 - NumberOfLeadingZeros((uint)m));
                int num3 = array2.Length * 8;
                for (int i = 0; i < num3 / num2; i++)
                {
                    if (num >= k)
                    {
                        break;
                    }

                    int from = i * num2;
                    int to = (i + 1) * num2;
                    int num4 = BitToIntOne(bit, from, to);
                    if (num4 < m)
                    {
                        array[num] = num4;
                        num++;
                    }
                }
            }

            return array;

        }

        public static uint NumberOfLeadingZeros(uint i)
        {
            if (i == 0)
            {
                return 32u;
            }

            uint num = 1u;
            if (i >> 16 == 0)
            {
                num += 16;
                i <<= 16;
            }

            if (i >> 24 == 0)
            {
                num += 8;
                i <<= 8;
            }

            if (i >> 28 == 0)
            {
                num += 4;
                i <<= 4;
            }

            if (i >> 30 == 0)
            {
                num += 2;
                i <<= 2;
            }

            return num - (i >> 31);
        }

        public static int BitToIntOne(BitArray bit, int from, int to)
        {
            int num = to - from;
            int count = bit.Count;
            int num2 = 0;
            for (int i = 0; i < num && i < count && i < 32; i++)
            {
                num2 = (bit[i + from] ? (num2 + (1 << i)) : num2);
            }

            return num2;
        }

        public static BitArray Reverse(this BitArray array)
        {
            int length = array.Length;
            int mid = (length / 2);

            for (int i = 0; i < mid; i++)
            {
                bool bit = array[i];
                array[i] = array[length - i - 1];
                array[length - i - 1] = bit;
            }

            return new BitArray(array);
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

        //
        // Summary:
        //     Calculates the optimal size of the bloom filter in bits given expectedElements
        //     (expected number of elements in bloom filter) and falsePositiveProbability (tolerable
        //     false positive rate).
        //
        // Parameters:
        //   n:
        //     Expected number of elements inserted in the bloom filter
        //
        //   p:
        //     Tolerable false positive rate
        //
        // Returns:
        //     the optimal siz of the bloom filter in bits
        public static int BestM(long n, double p)
        {
            return (int)Math.Ceiling(-1.0 * ((double)n * Math.Log(p)) / Math.Pow(Math.Log(2.0), 2.0));
        }

        //
        // Summary:
        //     Calculates the optimal hashes (number of hash function) given expectedElements
        //     (expected number of elements in bloom filter) and size (size of bloom filter
        //     in bits).
        //
        // Parameters:
        //   n:
        //     Expected number of elements inserted in the bloom filter
        //
        //   m:
        //     The size of the bloom filter in bits.
        //
        // Returns:
        //     the optimal amount of hash functions hashes
        public static int BestK(long n, long m)
        {
            return (int)Math.Ceiling(Math.Log(2.0) * (double)m / (double)n);
        }

        //
        // Summary:
        //     Calculates the amount of elements a Bloom filter for which the given configuration
        //     of size and hashes is optimal.
        //
        // Parameters:
        //   k:
        //     number of hashes
        //
        //   m:
        //     The size of the bloom filter in bits.
        //
        // Returns:
        //     mount of elements a Bloom filter for which the given configuration of size and
        //     hashes is optimal
        public static int BestN(long k, long m)
        {
            return (int)Math.Ceiling(Math.Log(2.0) * (double)m / (double)k);
        }

        //
        // Summary:
        //     Calculates the best-case (uniform hash function) false positive probability.
        //
        // Parameters:
        //   k:
        //     number of hashes
        //
        //   m:
        //     The size of the bloom filter in bits.
        //
        //   insertedElements:
        //     number of elements inserted in the filter
        //
        // Returns:
        //     The calculated false positive probability
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
