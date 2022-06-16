﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BloomFilter;
using FHICORC.Application.Models;
using FHICORC.Domain.Models;
using FHICORC.Infrastructure.Database.Context;

namespace FHICORC.Tests.ComponentTests.DGCGComponentTests
{
    public static class MobileUtil
    {

        public static bool ContainsCertificateFilterMobile(string certificateIdentifierHash, string signatureHash, IEnumerable<RevocationSuperFilter> revocationBatches, List<BucketItem> bloomFilterBuckets)
        {
            var allHashFunctionCertificateIdentifierIndicies_k = CalculateAllIndicies(certificateIdentifierHash, bloomFilterBuckets);
            var allHashFunctionSignatureIndicies_k = CalculateAllIndicies(signatureHash, bloomFilterBuckets);
            return CheckFilterByCountry(allHashFunctionCertificateIdentifierIndicies_k, allHashFunctionSignatureIndicies_k, revocationBatches);
        }

        public static bool CheckFilterByCountryParallel(List<int[]> allHashFunctionCertificateIdentifierIndicies_k, List<int[]> allHashFunctionSignatureIndicies_k, IEnumerable<RevocationSuperFilter> revocationBatches)
        {

            bool contains = revocationBatches.AsParallel()
                .Any(r => BatchContains(r, allHashFunctionCertificateIdentifierIndicies_k, allHashFunctionSignatureIndicies_k));
            return contains;
        }

        public static bool CheckFilterByCountry(List<int[]> allHashFunctionCertificateIdentifierIndicies_k, List<int[]> allHashFunctionSignatureIndicies_k, IEnumerable<RevocationSuperFilter> revocationBatches)
        {
            foreach (var r in revocationBatches)
            {
                var bitVector = new BitArray(r.SuperFilter);

                bool contains;
                if (r.HashType == 1)
                {
                    contains = bitVector.Contains(allHashFunctionSignatureIndicies_k[r.Bucket]);
                }
                else
                {
                    contains = bitVector.Contains(allHashFunctionCertificateIdentifierIndicies_k[r.Bucket]);
                }

                if (contains)
                    return true;
            }

            return false;
        }


        public static bool BatchContains(RevocationSuperFilter revocationBatch, List<int[]> allHashFunctionCertificateIdentifierIndicies_k, List<int[]> allHashFunctionSignatureIndicies_k)
        {

            var bitVector = new BitArray(revocationBatch.SuperFilter);

            bool contains;
            if (revocationBatch.HashType == 1)
            {
                contains = bitVector.Contains(allHashFunctionSignatureIndicies_k[revocationBatch.Bucket]);
            }
            else
            {
                contains = bitVector.Contains(allHashFunctionCertificateIdentifierIndicies_k[revocationBatch.Bucket]);
            }

            return contains;
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


        public static List<int[]> CalculateAllIndicies(string hashString, IEnumerable<BucketItem> bloomFilterBuckets)
        {
            var allHashFunctionIndicies_k = new List<int[]>();
            foreach (var bucketItem in bloomFilterBuckets)
            {
                var hashedIndicies = HashData(Encoding.UTF8.GetBytes(hashString), bucketItem.BitVectorLength_m, bucketItem.NumberOfHashFunctions_k);
                allHashFunctionIndicies_k.Add(hashedIndicies);
            };

            return allHashFunctionIndicies_k;
        }


    }
}
