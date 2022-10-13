using FHICORC.Core.Services.Enum;
using System;

namespace FHICORC.Application.Models
{
    public class SuperBatch{

        public SuperBatch(int id, string countryISO3166, int bucketType, byte[] bloomFilter, HashTypeEnum hashType, DateTime expirationDate) { 
            I = id;
            C = countryISO3166;
            B = bucketType;
            F = bloomFilter;
            H = hashType;
            E = expirationDate;
        }

        // Id
        public int I { get; private set; }
        // CountryISO3166
        public string C { get; private set; }
        // BucketType
        public int B { get; private set; }
        // BloomFilter
        public byte[] F { get; private set; }
        // HashType
        public HashTypeEnum H { get; private set; }
        // ExpirationDate
        public DateTime E { get; private set; }

    }
}
