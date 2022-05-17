using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Domain.Models;
using FHICORC.Infrastructure.Database.Context;
using FHICORC.Integrations.DGCGateway.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoreLinq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FHICORC.Application.Services
{
    public class RevocationService : IRevocationService
    {
        private readonly ILogger<RevocationService> _logger;
        private readonly CoronapassContext _coronapassContext;
        private readonly BatchOptions _valueBatchOptions;
        private readonly DateTime _expiryDateInThreeMonth = DateTime.Now.AddMonths(3).Date;

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

        public void UploadHashes(IEnumerable<string> newHashes)
        {
            var hashesWithoutBatch = GetHashesWithoutRevocationbatch(newHashes).ToList();
            var currentBatch = FetchExistingBatch() ?? AddBatch();

            foreach (var hash in hashesWithoutBatch)
            {
                if (currentBatch.Count < 1000)
                {
                    AddHashToBatch(currentBatch.BatchId, hash);
                    currentBatch.Count++;
                }
                else
                {
                    currentBatch = AddBatch();
                    AddHashToBatch(currentBatch.BatchId, hash);
                    currentBatch.Count++;
                }
            }
            _coronapassContext.SaveChanges();
        }

        private void AddHashToBatch(string batchId, string hash)
        {
            var hashDto = new RevocationHash
            {
                BatchId = batchId,
                Hash = hash,
            };
            try
            {
                _coronapassContext.RevocationHash.Add(hashDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception when trying to create a hash with BatchId: {batchId}, message: {ex.Message}");
            }
        }
        private BatchItem AddBatch()
        {
            var newBatch = new RevocationBatch
            {
                BatchId = Guid.NewGuid().ToString(),
                Expires = _expiryDateInThreeMonth,
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
                _logger.LogError($"Exception when trying to create a batch with BatchId: {newBatch.BatchId}, message: {ex.Message}");
            }

            return new BatchItem(newBatch.BatchId, 0, newBatch.Expires);

        }

        private List<string> GetHashesWithoutRevocationbatch(IEnumerable<string> hashList)
        {
            var revokedHashList = _coronapassContext.RevocationHash.Select(x => new HashDto(x)).ToList();

            return hashList.Where(h => revokedHashList.All(rh => rh.HashInfo != h)).ToList();
        }
        

        private BatchItem FetchExistingBatch()
        {
            return _coronapassContext.RevocationBatch.Include(x => x.RevocationHashes)
                .Where(x => x.Country != null && x.Country.Equals(_valueBatchOptions.CountryCode) && x.Expires.Date == _expiryDateInThreeMonth && x.RevocationHashes.Count!= 1000)
                .Select(y => new BatchItem(y.BatchId, Convert.ToInt32(y.RevocationHashes.Count()), y.Expires))
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
