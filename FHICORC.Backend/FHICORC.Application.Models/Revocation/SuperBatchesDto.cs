using FHICORC.Core.Services.Enum;
using System;

namespace FHICORC.Application.Models
{
    public class SuperBatch{

        public SuperBatch(int id, string countryISO3166, int bucketType, byte[] bloomFilter, HashTypeEnum hashType, DateTime expirationDate) { 
            Id = id;
            CountryISO3166 = countryISO3166;
            BucketType = bucketType;
            BloomFilter = bloomFilter;
            HashType = hashType;
            ExpirationDate = expirationDate;
        }

        public int Id { get; private set; }
        public string CountryISO3166 { get; private set; }
        public int BucketType { get; private set; }
        public byte[] BloomFilter { get; private set; }
        public HashTypeEnum HashType { get; private set; }
        public DateTime ExpirationDate { get; private set; }

    }
}
