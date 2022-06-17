using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Domain.Models;
using FHICORC.Infrastructure.Database.Context;
using FHICORC.Integrations.DGCGateway.Util;
using FHICORC.Application.Models;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using System.Collections.Generic;
using FHICORC.Core.Services.Enum;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace FHICORC.Application.Services
{
    public class RevocationUploadService : IRevocationUploadService
    {
        private readonly ILogger<RevocationUploadService> _logger;
        private readonly CoronapassContext _coronapassContext;
        private readonly BatchOptions _valueBatchOptions;
        private readonly DateTime _expiryDateInThreeMonth = DateTime.Now.AddMonths(3).Date;

        public RevocationUploadService(ILogger<RevocationUploadService> logger, CoronapassContext coronapassContext, BatchOptions batchOptions)
        {
            _logger = logger;
            _coronapassContext = coronapassContext;
            _valueBatchOptions = batchOptions;
        }


        public bool UploadHashes(IEnumerable<string> newHashes)
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
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception when trying to update Database, message: {ex.Message}");
                return false;
            }
        }

        private void AddHashToBatch(string batchId, string hash)
        {
            var newHash = new RevocationHash(batchId, hash);

            _coronapassContext.RevocationHash.Add(newHash);
        }

        private BatchItem AddBatch()
        {
            var newBatch = new RevocationBatch(Guid.NewGuid().ToString(), _expiryDateInThreeMonth, _valueBatchOptions.CountryCode, false, _valueBatchOptions.HashType.ParseHashTypeToEnum(), true);

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
