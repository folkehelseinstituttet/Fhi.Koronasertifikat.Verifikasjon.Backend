using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BloomFilter;
using FHICORC.Application.Models;
using FHICORC.Infrastructure.Database.Context;

namespace FHICORC.Tests.ComponentTests.DGCGComponentTests
{
    public static class MobileUtil
    {

        public static bool ContainsCertificateFilterMobile(string country, string hashString, CoronapassContext _coronapassContext, BloomFilterBuckets bloomFilterBuckets)
        {
            var allHashFunctionIndicies_k = CalculateAllIndicies(hashString, bloomFilterBuckets);
            return CheckFilterByCountry(country, _coronapassContext, allHashFunctionIndicies_k);
        }


        public static bool CheckFilterByCountry(string country, CoronapassContext _coronapassContext, List<int[]> allHashFunctionIndicies_k) {

            var superFilter = _coronapassContext.RevocationSuperFilter
                .Where(s => s.SuperCountry.Equals(country));

            foreach (var s in superFilter)
            {
                var bitVector = new BitArray(s.SuperFilter);
                var contains = bitVector.Contains(allHashFunctionIndicies_k[s.Bucket]);

                if (contains)
                    return true;
            }

            return false;
        }




        public static bool Contains(this BitArray filter, int[] indicies)
        {
            foreach (int i in indicies)
            {
                if (!filter[i])
                    return false;
            }
            return true;
        }


        public static int[] HashData(byte[] data, int m, int k)
        {
            var hashFunction = HashFunction.Functions[HashMethod.Murmur3KirschMitzenmacher];
            return hashFunction.ComputeHash(data, m, k);
        }


        public static List<int[]> CalculateAllIndicies(string hashString, BloomFilterBuckets bloomFilterBuckets)
        {
            var allHashFunctionIndicies_k = new List<int[]>();
            foreach (var bucketItem in bloomFilterBuckets.Buckets)
            {
                var hashedIndicies = HashData(Encoding.UTF8.GetBytes(hashString), bucketItem.BitVectorLength_m, bucketItem.NumberOfHashFunctions_k);
                allHashFunctionIndicies_k.Add(hashedIndicies);
            };

            return allHashFunctionIndicies_k;
        }


    }
}
