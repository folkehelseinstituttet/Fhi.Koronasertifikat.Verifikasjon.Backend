using System.Text;
using System.Collections;
using System;
using BloomFilter;
using System.Collections.Generic;
using FHICORC.Infrastructure.Database.Context;
using FHICORC.Application.Models;
using System.Linq;
using FHICORC.Core.Services.Enum;

namespace FHICORC.Integrations.DGCGateway.Util
{
    public static class BloomFilterUtils
    {

        public static bool IsHashRevocated(string hashString, string country, CoronapassContext _coronapassContext, BloomFilterBuckets bloomFilterBuckets)
        {
            var allHashFunctionIndicies_k = CalculateAllHashIndiciesByBucket(hashString, bloomFilterBuckets);
            return SuperFilterContains(allHashFunctionIndicies_k, country, _coronapassContext);
        }


        public static bool SuperFilterContains(List<int[]> allHashFunctionIndicies_k, string country, CoronapassContext _coronapassContext)
        {

            var superFilter = _coronapassContext.RevocationSuperFilter
                .Where(s => s.SuperCountry.Equals(country));

            foreach (var s in superFilter)
            {
                var bitVector = new BitArray(s.SuperFilter);
                var contains = bitVector.BitVectorContains(allHashFunctionIndicies_k[s.Bucket]);

                if (contains)
                    return true;
            }

            return false;
        }



        public static bool BitVectorContains(this BitArray filter, int[] indicies)
        {
            foreach (int i in indicies)
            {
                if (!filter[i])
                    return false;
            }
            return true;
        }


        public static List<int[]> CalculateAllHashIndiciesByBucket(string hashString, BloomFilterBuckets bloomFilterBuckets)
        {
            var allHashFunctionIndicies_k = new List<int[]>();
            foreach (var bucketItem in bloomFilterBuckets.Buckets)
            {
                var hashedIndicies = HashData(Encoding.UTF8.GetBytes(hashString), bucketItem.BitVectorLength_m, bucketItem.NumberOfHashFunctions_k);
                allHashFunctionIndicies_k.Add(hashedIndicies);
            };

            return allHashFunctionIndicies_k;
        }



        public static BitArray AddToFilter(this BitArray filter, string str, int m, int k)
        {
            var hash = HashData(Encoding.UTF8.GetBytes(str), m, k);

            foreach (int i in hash)
                filter[i] = true;

            return filter;

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

        public static string ToTitleCase(this string str)
        {
            if (str.Length == 0)
                return str;
            else if (str.Length == 1)
                return char.ToUpper(str[0]) + "";
            else
                return char.ToUpper(str[0]) + str[1..].ToLower();
        }

        public static int ParseHashTypeToEnum(this string hashType) {
            return (int)(Enum.TryParse(hashType, out HashType myHashType) ? myHashType : 0);
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
