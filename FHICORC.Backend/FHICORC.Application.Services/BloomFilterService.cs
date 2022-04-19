using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using FHICORC.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace FHICORC.Application.Services
{
    public class BloomFilterService : IBloomFilterService
    {
        private readonly ILogger<BloomFilterService> _logger;
        private readonly CoronapassContext _coronapassContext;

        public BloomFilterService(ILogger<BloomFilterService> logger, CoronapassContext coronapassContext)
        {
            _logger = logger;
            _coronapassContext = coronapassContext;
        }

        private static Random random = new Random();

        public void CustomeFilter()
        {
            var m = 47936;
            var k = 32;
            var bloomStats = BloomFilterUtils.CalcOptimalNP(m, k);

            //var bloomStats = BloomFilterUtils.CalcOptimalMK(1000, 1E-10);
            //var m = bloomStats.m;
            //var k = bloomStats.k;

            var str = "whatssup";

            var filter = new BitArray(m);

            filter.AddToFilter(str, m, k);
            filter.AddToFilter("hello", m, k);
            filter.AddToFilter("nahanaa", m, k);

            var contains = filter.Contains(str, m, k);

        }

        public bool ContainsCertificate() {

            var str = "nononono";
            var testHash = sha256_hash(str);

            return ContainsCertificateFilter(testHash);
        }

        public bool ContainsCertificateFilter(string str) {
            var hashData = BloomFilterUtils.HashData(Encoding.UTF8.GetBytes(str), 47936, 32);

            var listOfBatchIds = new List<int>();

            var now = DateTime.UtcNow;
            var relevantFilters = _coronapassContext.FiltersRevoc
                .Where(b => !b.BatchesRevoc.Deleted && b.BatchesRevoc.Expires >= now);

            foreach (var bf in relevantFilters)
            {
                var contains = new BitArray(bf.Filter).Contains(hashData);
                if (contains)
                    listOfBatchIds.Add(bf.BatchId);
            }

            if (listOfBatchIds.Count == 0)
                return false;

            return ContainsCertificateBatch(listOfBatchIds, str);


        }

        public bool ContainsCertificateBatch(List<int> batchIds, string str) {

            var b = _coronapassContext.BatchesRevoc
                .FirstOrDefault(b => batchIds.Contains(b.BatchId) && b.HashesRevoc.Hash == str);


            return b != null;
        }


        public void AddToFilterTest(int numberOfHashes=1000) 
        {
            var m = 47936;
            var k = 32;
            var bloomStats = BloomFilterUtils.CalcOptimalNP(m, k);

            var filter = new BitArray(m);

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            for (int i = 0; i < numberOfHashes; i++) {
                var str = new string(Enumerable.Repeat(chars, random.Next(8, 100))
                    .Select(s => s[random.Next(s.Length)]).ToArray());

                var element = sha256_hash(str);
                filter.AddToFilter(element, m, k);
            }


            var testStr = "helloWhatsippp";
            var testHash = sha256_hash(testStr);
            filter.AddToFilter(testHash, m, k);

            testStr = "nononono";
            testHash = sha256_hash(testStr);
            filter.AddToFilter(testHash, m, k);

            byte[] filterBytes = new byte[filter.Length / 8];
            filter.CopyTo(filterBytes, 0);


            var record = new Domain.Models.FiltersRevoc { BatchId = 3, Filter = filterBytes };
            var aExists = _coronapassContext.FiltersRevoc.Find(record.BatchId);

            if (aExists == null)
            {
                _coronapassContext.FiltersRevoc.Add(record);
            }
            else
            {
                _coronapassContext.Entry(aExists).State = EntityState.Detached;
                _coronapassContext.Entry(record).State = EntityState.Modified;
            }

            _coronapassContext.SaveChanges();

        }


        public static string sha256_hash(string value)
        {
            using var hash = SHA256.Create();
            var byteArray = hash.ComputeHash(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(byteArray).ToLower();
        }

    }
}
