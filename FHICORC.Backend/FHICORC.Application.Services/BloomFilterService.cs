using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using FHICORC.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using FHICORC.Application.Models.Revocation;
using FHICORC.Domain.Models;

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

        public bool ContainsCertificate(string dcc) {
            return ContainsCertificateFilter(dcc);
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
            foreach (var id in batchIds) {
                var existingHash = _coronapassContext.HashesRevoc
                    .FirstOrDefault(h => h.Hash.Equals(str));

                if (existingHash != null)
                    return true;
            }
            
            return false;
        }


        public List<FilterRevocDto> GetFilterRevocList() {


            var _tmp = new List<FilterRevocDto>();
            foreach (var fr in _coronapassContext.FiltersRevoc) {
                var _fr = new FilterRevocDto()
                {
                    BatchId = fr.BatchId,
                    Filter = fr.Filter,
                    Date = DateTime.UtcNow,
                };

                _tmp.Add(_fr);
            }

            return _tmp;
        }

        public void AddToRevocation(string dcc) {

            if (ContainsCertificate(dcc))
                return;


            var newBatch = new BatchesRevoc()
            {
                Country = "Norway",
                Date = DateTime.UtcNow.Date,
                Expires = DateTime.UtcNow.AddDays(10).Date,
                Deleted = false,
                Upload = true,
                Kid = "asdsadsadsa",
                HashType = "Sha256"

            };

            var existingBatches = _coronapassContext.BatchesRevoc
                .Where(b => b.Upload
                && !b.Deleted
                && b.Country == newBatch.Country
                && b.Expires == newBatch.Expires
                && b.HashType == newBatch.HashType)
                .ToList();

            foreach (var b in existingBatches) {

                var cnt = _coronapassContext.HashesRevoc
                    .Where(h => h.BatchId == b.BatchId)
                    .Count();

                if (cnt > 1000)
                    continue;


                var _newHash = new HashesRevoc() {
                    BatchId = b.BatchId,
                    Hash = dcc
                };
                _coronapassContext.HashesRevoc.Add(_newHash);
                _coronapassContext.SaveChanges();

                AddToFilter(b.BatchId, dcc);
                return;
            }


            _coronapassContext.BatchesRevoc.Add(newBatch);
            _coronapassContext.SaveChanges();

            var newHash = new HashesRevoc()
            {
                BatchId = newBatch.BatchId,
                Hash = dcc
            };
            _coronapassContext.HashesRevoc.Add(newHash);
            _coronapassContext.SaveChanges();

            AddToFilter(newBatch.BatchId, dcc);

        }

        public void AddToFilter(int batchId, string dcc) {
            var existingFilter = _coronapassContext.FiltersRevoc.Find(batchId);
            var m = 47936;
            var k = 32;
            //var bloomStats = BloomFilterUtils.CalcOptimalNP(m, k);

            var filter = existingFilter == null ? new BitArray(m) : new BitArray(existingFilter.Filter);
            filter.AddToFilter(dcc, m, k);

            var filterBytes = new byte[filter.Length / 8];
            filter.CopyTo(filterBytes, 0);
            var newFilter = new FiltersRevoc { BatchId = batchId, Filter = filterBytes };

            if (existingFilter == null)
            {
                _coronapassContext.FiltersRevoc.Add(newFilter);
            }
            else
            {
                _coronapassContext.Entry(existingFilter).State = EntityState.Detached;
                _coronapassContext.Entry(newFilter).State = EntityState.Modified;
            }
            _coronapassContext.SaveChanges();
        }

        public void CreateSuperFilter()
        {

            var dateBuffer = 10;

            _coronapassContext.BatchesRevoc
                .Where(b => b.SuperFiltersRevocId == null);









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

            filter.AddToFilter("wow", m, k);
            filter.AddToFilter("sup", m, k);
            filter.AddToFilter("yeah", m, k);


            byte[] filterBytes = new byte[filter.Length / 8];
            filter.CopyTo(filterBytes, 0);


            var record = new Domain.Models.FiltersRevoc { BatchId = 2, Filter = filterBytes };
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
