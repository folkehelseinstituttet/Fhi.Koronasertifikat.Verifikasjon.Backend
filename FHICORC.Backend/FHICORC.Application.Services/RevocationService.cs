using System.Text;
using System.Collections;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using FHICORC.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using FHICORC.Integrations.DGCGateway.Util;
using FHICORC.Application.Models;
using System.Collections.Generic;
using FHICORC.Domain.Models;
using MoreLinq;

namespace FHICORC.Application.Services
{
    public class RevocationService : IRevocationService
    {
        private readonly ILogger<RevocationService> _logger;
        private readonly CoronapassContext _coronapassContext;

        private readonly int MAX_SIZE_BATCH = 1000;
        private readonly string COUNTRY_CODE = "NO";
        private readonly string HASH_TYPE = "COUNTRYCODEUCI";
        private readonly DateTime ExpiryDate = DateTime.Now.AddMonths(3).Date;

        public RevocationService(ILogger<RevocationService> logger, CoronapassContext coronapassContext)
        {
            _logger = logger;
            _coronapassContext = coronapassContext;
        }

        public bool ContainsCertificate(string dcc) {
            return ContainsCertificateFilter(dcc);
        }

        public bool ContainsCertificateFilter(string str) {
            var hashData = BloomFilterUtils.HashData(Encoding.UTF8.GetBytes(str), 47936, 32);

            foreach (var bf in _coronapassContext.RevocationSuperFilter)
            {
                var a = new BitArray(bf.SuperFilter);
                var contains = a.Contains(hashData);
                if (contains)
                    return true;
            }
            return false;
        }

        public SuperBatchesDto FetchSuperBatches(DateTime dateTime) {
            var superBatchList = _coronapassContext.RevocationSuperFilter
                .Where(s => s.Modified <= dateTime)
                .Select(x => new SuperBatch()
                {
                    Id = x.Id,
                    SuperFilter = x.SuperFilter,
                }
                ).ToList();

            return new SuperBatchesDto()
            {
                SuperBatches = superBatchList
            };           
        }

        public void UploadHashes(List<string> hashList)
        {
            // List of all Hashes
            var revokedHashList = FetchHashes();

            // Compare List of Hashes from input to HashesRevoc table in DB
            var newHashList = hashList
                .Where(h => revokedHashList.Hashes
                .All(rh => rh.HashInfo != h))
                .ToList();

            var batchItem = FetchSmallestBatchItem();

            Guid newGuid = Guid.NewGuid();
            bool expiresMatch = false;
            bool firstIteration = true;
            int counter = 1;
            int batchItemCount = 0;
            int countHashList = newHashList.Count;

            if (batchItem != null)
            {
                expiresMatch = ExpiryDate == batchItem.Expires.Date;
                batchItemCount = batchItem.Count;
            }

            foreach (var hash in newHashList.Select((value, index) => new { value, index }))
            {
                if (hash.index < (MAX_SIZE_BATCH - batchItemCount) && expiresMatch)
                {
                    CreateHash(batchItem.BatchId, hash.value);
                }
                else if (counter < MAX_SIZE_BATCH)
                {
                    if (firstIteration)
                    {
                        CreateBatch(newGuid.ToString());
                        firstIteration = false;
                        counter = 0;
                    }
                    CreateHash(newGuid.ToString(), hash.value);
                    counter++;
                }
                else
                {   
                    newGuid = Guid.NewGuid();
                    counter = 1;
                    
                    CreateBatch(newGuid.ToString());
                    CreateHash(newGuid.ToString(), hash.value);
                }
            }
        }

        private void CreateHash(string batchId, string hash)
        {
            var hashDto = new HashesRevoc
            {
                BatchId = batchId,
                Hash = hash,
            };
            _coronapassContext.ChangeTracker.Clear();
            _coronapassContext.HashesRevoc.Add(hashDto);
            _coronapassContext.SaveChanges();
        }
        private void CreateBatch(string batchId)
        {
            var newBatch = new BatchesRevoc
            {
                BatchId = batchId,
                Expires = DateTime.Now.AddMonths(3),
                Country = COUNTRY_CODE,
                HashType = HASH_TYPE,
                Deleted = false,
                Upload = true,
            };
            _coronapassContext.ChangeTracker.Clear();
            _coronapassContext.BatchesRevoc.Add(newBatch);
            try
            {

                _coronapassContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Duplicate Key: {batchId}, message: {ex.Message}");
            }
        }
        private HashesDto FetchHashes()
        {
            var hashList = _coronapassContext.HashesRevoc
                .Select(x => new Hash()
                {
                    Id = x.Id,
                    BatchId = x.BatchId,
                    HashInfo = x.Hash,
                }
                ).ToList();

            return new HashesDto()
            {
                Hashes = hashList
            };
        }
        private BatchItem FetchSmallestBatchItem()
        {
            var batchItem = _coronapassContext.BatchesRevoc.Join(_coronapassContext.HashesRevoc,
            b => b.BatchId,
            h => h.BatchId, (b, h) => new { Batch = b, Hash = h })
            .Where(x => x.Batch.Country.Equals(COUNTRY_CODE) && x.Batch.Expires.Date == ExpiryDate)
            .GroupBy(y => new
            {
                y.Batch.BatchId,
                y.Batch.Expires,
            })
            .Where(x => x.Count() < MAX_SIZE_BATCH)
            .Select(y => new BatchItem
            {
                BatchId = y.Key.BatchId,
                Count = y.Count(),
                Expires = y.Key.Expires,
            })
            .OrderBy(x => x.Count)
            .FirstOrDefault();

            return batchItem;
        }
        class BatchItem
        {
            public string BatchId { get; set; }
            public int Count { get; set; }
            public DateTime Expires { get; set; }
        }

    }
}
