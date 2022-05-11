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
using FHICORC.Application.Models.Options;

namespace FHICORC.Application.Services
{
    public class RevocationService : IRevocationService
    {
        private readonly ILogger<RevocationService> _logger;
        private readonly CoronapassContext _coronapassContext;
        private readonly BatchOptions _valueBatchOptions;
        private readonly DateTime ExpiryDate = DateTime.Now.AddMonths(3).Date;

        public RevocationService(ILogger<RevocationService> logger, CoronapassContext coronapassContext, BatchOptions batchOptions)
        {
            _logger = logger;
            _coronapassContext = coronapassContext;
            _valueBatchOptions = batchOptions;
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

        public void UploadHashes(IEnumerable<string> hashList)
        {
            var revokedHashList = FetchHashes1();
            var newHashList = ReturnUniqueHashes(hashList, revokedHashList).ToList();
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
                if (hash.index < (_valueBatchOptions.BatchSize - batchItemCount) && expiresMatch)
                {
                    CreateHash(batchItem.BatchId, hash.value);
                }
                else if (counter < _valueBatchOptions.BatchSize)
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
            _coronapassContext.SaveChanges();
        }

        private void CreateHash(string batchId, string hash)
        {
            var hashDto = new RevocationHash
            {
                BatchId = batchId,
                Hash = hash,
            };
            _coronapassContext.RevocationHash.Add(hashDto);
        }
        private void CreateBatch(string batchId)
        {
            var newBatch = new RevocationBatch
            {
                BatchId = batchId,
                Expires = DateTime.Now.AddMonths(3),
                Country = _valueBatchOptions.CountryCode,
                HashType = _valueBatchOptions.HashType,
                Deleted = false,
                Upload = true,
            };
            _coronapassContext.RevocationBatch.Add(newBatch);
            try
            {
                _coronapassContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception with BatchId: {batchId}, message: {ex.Message}");
            }
        }
        private List<string> ReturnUniqueHashes(IEnumerable<string> hashList, IEnumerable<HashDto> revokedHashList) => hashList.Where(h => revokedHashList.All(rh => rh.HashInfo != h)).ToList();        

        private List<HashDto> FetchHashes1() => _coronapassContext.RevocationHash.Select(x => new HashDto(x)).ToList();

        private BatchItem FetchSmallestBatchItem()
        {
            return _coronapassContext.RevocationBatch.ToList()
                .Join(_coronapassContext.RevocationHash,
                    b => b.BatchId,
                    h => h.BatchId, (b, h) => new { Batch = b, Hash = h })
                .Where(x => x.Batch.Country != null && x.Batch.Country.Equals(_valueBatchOptions.CountryCode) && x.Batch.Expires.Date == ExpiryDate)
                .GroupBy(y => new
                {
                    y.Batch.BatchId,
                    y.Batch.Expires,
                })
                .Where(x => x.Count() < _valueBatchOptions.BatchSize)
                .Select(y => new BatchItem(y.Key.BatchId, y.Count(), y.Key.Expires))
                .OrderBy(x => x.Count)
                .FirstOrDefault();
        }

        private class BatchItem
        {
            public BatchItem(string BatchId, int Count, DateTime Expires)
            {
                this.BatchId = BatchId;
                this.Count = Count;
                this.Expires = Expires;
            }
            public string BatchId { get; set; }
            public int Count { get; set; }
            public DateTime Expires { get; set; }
        }

    }
}
