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

        public bool ContainsCertificate(string dcc)
        {
            return ContainsCertificateFilter(dcc);
        }

        public bool ContainsCertificateFilter(string str)
        {
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

        public SuperBatchesDto FetchSuperBatches(DateTime dateTime)
        {
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
                if (currentBatch.Count < _valueBatchOptions.BatchSize)
                {
                    AddHashToBatch(currentBatch.BatchId, hash);
                    currentBatch.IncrementCount();
                }
                else
                {
                    currentBatch = AddBatch();
                    AddHashToBatch(currentBatch.BatchId, hash);
                    currentBatch.IncrementCount();
                }
            }

            try
            {
                _coronapassContext.SaveChanges();

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception when trying to update Database, message: {ex.Message}");
            }
        }

        private void AddHashToBatch(string batchId, string hash)
        {
            var newHash = new RevocationHash(batchId, hash);

            _coronapassContext.RevocationHash.Add(newHash);
        }

        private BatchItem AddBatch()
        {
            var newBatch = new RevocationBatch(Guid.NewGuid().ToString(), _expiryDateInThreeMonth, _valueBatchOptions.CountryCode, false, _valueBatchOptions.HashType, true);
          
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

        private IEnumerable<string> GetHashesWithoutRevocationbatch(IEnumerable<string> hashList)
        {
            var revokedHashList = _coronapassContext.RevocationHash.Select(x => new HashDto(x)).ToList();

            return hashList.Where(h => revokedHashList.All(rh => rh.HashInfo != h));
        }

        private BatchItem FetchExistingBatch()
        {
            return _coronapassContext.RevocationBatch.Include(x => x.RevocationHashes)
                .Where(x => x.Country != null && x.Country.Equals(_valueBatchOptions.CountryCode) && x.Expires.Date == _expiryDateInThreeMonth && x.RevocationHashes.Count != _valueBatchOptions.BatchSize)
                .Select(y => new BatchItem(y.BatchId, Convert.ToInt32(y.RevocationHashes.Count()), y.Expires))
                .FirstOrDefault();
        }

        private class BatchItem
        {
            public BatchItem(string batchId, int count, DateTime expires)
            {
                this.BatchId = batchId;
                this.Count = count;
                this.Expires = expires;
            }

            public string BatchId { get; private set; }
            public int Count { get; private set; }
            public DateTime Expires { get; private set; }

            public void IncrementCount()
            {
                this.Count++;
            }
        }

    }
}
