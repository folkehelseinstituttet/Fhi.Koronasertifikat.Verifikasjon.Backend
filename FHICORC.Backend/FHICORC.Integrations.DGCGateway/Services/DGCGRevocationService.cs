using System.Collections;
using System;
using Microsoft.Extensions.Logging;
using FHICORC.Domain.Models;
using FHICORC.Application.Models;
using FHICORC.Infrastructure.Database.Context;
using FHICORC.Integrations.DGCGateway.Util;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using System.Threading.Tasks;
using FHICORC.Application.Models.Options;

namespace FHICORC.Integrations.DGCGateway.Services
{
    public class DGCGRevocationService : IDGCGRevocationService
    {
        private readonly ILogger<DGCGRevocationService> _logger;
        private readonly CoronapassContext _coronapassContext;
        private readonly IDgcgService _dgcgService;
        private readonly BloomBucketOptions _bloomBucketOptions; 
        private readonly IBloomBucketService _bloomBucketService;

        public DGCGRevocationService(ILogger<DGCGRevocationService> logger, CoronapassContext coronapassContext, IDgcgService dgcgService, BloomBucketOptions bloomBucketOptions, IBloomBucketService bloomBucketService)
        {
            _logger = logger;
            _coronapassContext = coronapassContext;
            _dgcgService = dgcgService;
            _bloomBucketOptions = bloomBucketOptions;
            _bloomBucketService = bloomBucketService;
        }

        public async Task PopulateRevocationDatabase(DgcgRevocationBatchListRespondDto revocationBatchList) {
            if (revocationBatchList is null) {
                _logger.LogInformation("RevocationBatchList is empty");
                return;
            }

            foreach (var rb in revocationBatchList.Batches)
            {
                try
                {
                    var revocationHashList = await _dgcgService.GetRevocationBatchAsync(rb.BatchId);
                    AddToDatabase(rb, revocationHashList);
                }
                catch (System.Security.Cryptography.CryptographicException e)
                {
                    _logger.LogInformation(e, "No/Invalid data recevied from the DCCG for batch {batchId}", rb.BatchId);
                }
                catch (Exception e) {
                    _logger.LogInformation(e, "Failed to Add Batch {BatchId} to the database", rb.BatchId);
                }
            }

            OrganizeBatches();

        }

        public void OrganizeBatches() {
            var revocationSuperFilters = _coronapassContext.RevocationSuperFilter
            .Where(s => s.Modified >= DateTime.UtcNow.AddHours(-10))
            .Include(r => r.RevocationBatches)
                .ThenInclude(h => h.RevocationHashes);

            foreach (var revocationSuperFilter in revocationSuperFilters)
            {
                //var bitVector = GenerateBitVectorForSingleSuperBatch(revocationSuperFilter.RevocationBatches, revocationSuperFilter.BatchCount);
                var bucket = _bloomBucketService.GetBucketItemByBatchCount(revocationSuperFilter.BatchCount);
                var bitVector = new BitArray(bucket.BitVectorLength_m);

                foreach (var revocationBatch in revocationSuperFilter.RevocationBatches)
                {
                    foreach (var hash in revocationBatch.RevocationHashes)
                    {
                        bitVector.AddToFilter(hash.Hash, bucket.BitVectorLength_m, bucket.NumberOfHashFunctions_k);
                    }
                }

                revocationSuperFilter.SuperFilter = BloomFilterUtils.BitToByteArray(bitVector);
                _coronapassContext.Entry(revocationSuperFilter).State = EntityState.Modified;
            }

            _coronapassContext.SaveChanges();

        }

        public void AddToDatabase(DgcgRevocationListBatchItem batchRoot, DGCGRevocationBatchRespondDto batch) {

            var batchId = batchRoot.BatchId;
            var revocationBatch = FillInBatchRevoc(batchRoot, batch);                
            var superId = FillInRevocationSuperFilter(batch);
            revocationBatch.SuperId = superId;

            _coronapassContext.RevocationBatch.Add(revocationBatch);
            AddHashRevoc(batchId, batch);
        }


        public static RevocationBatch FillInBatchRevoc(DgcgRevocationListBatchItem batchRoot, DGCGRevocationBatchRespondDto batch) {
            var revocationBatch = new RevocationBatch()
            {
                BatchId = batchRoot.BatchId,
                Expires = batch.Expires,
                Date = batchRoot.Date,
                Country = batchRoot.Country,
                Deleted = batchRoot.Deleted,
                Kid = batch.Kid,
                HashType = batch.HashType,
                Upload = false,
            };
            return revocationBatch;
        }

        private void AddHashRevoc(string batchId, DGCGRevocationBatchRespondDto batch) {
            foreach (var b in batch.Entries)
            {
                var _revocationHash = new RevocationHash()
                {
                    BatchId = batchId,
                    Hash = b.Hash
                };
                _coronapassContext.RevocationHash.Add(_revocationHash);
            }
        }

        public int FillInRevocationSuperFilter(DGCGRevocationBatchRespondDto batch){               

            var currenBatchCount = batch.Entries.Count;
            foreach (var su in _coronapassContext.RevocationSuperFilter)
            {
                if (su.SuperCountry == batch.Country) {
                    if (su.SuperExpires >= batch.Expires && batch.Expires >= su.SuperExpires.AddDays(-_bloomBucketOptions.ExpieryDateLeewayInDays)) {
                        foreach (var bucket in _bloomBucketService.GetBloomFilterBucket().Buckets) {
                            if (su.BatchCount + currenBatchCount <= bucket.MaxValue)
                            {
                                su.BatchCount += currenBatchCount;
                                su.Modified = DateTime.UtcNow;
                                su.Bucket = bucket.BucketId;

                                _coronapassContext.Entry(su).State = EntityState.Modified;
                                return su.Id;
                            }
                        }
                    }
                }
            }

            var revocationSuperFilter = new RevocationSuperFilter()
            {
                SuperCountry = batch.Country,
                SuperExpires = batch.Expires.AddDays(_bloomBucketOptions.ExpieryDateLeewayInDays).Date,
                BatchCount = currenBatchCount,
                Modified = DateTime.UtcNow,
                Bucket = _bloomBucketService.GetBucketItemByBatchCount(currenBatchCount).BucketId
            };

            _coronapassContext.RevocationSuperFilter.Add(revocationSuperFilter);
            _coronapassContext.SaveChanges();
            return revocationSuperFilter.Id;

        }



        public void DeleteExpiredBatches()
        {
            try
            {
                var batchesToDelete = _coronapassContext.RevocationBatch
                    .Where(b => !b.Deleted && b.Expires <= DateTime.UtcNow);

                var superBatchIdsToRecalculate = new HashSet<int>();
                foreach (var b in batchesToDelete)
                {
                    b.Deleted = true;

                    if (b.SuperId is not null)
                        superBatchIdsToRecalculate.Add((int)b.SuperId);

                    b.SuperId = null;
                    _coronapassContext.Entry(b).State = EntityState.Modified;
                }
                _coronapassContext.SaveChanges();

            }
            catch (Exception ex)
            {

            }
        }
    }

}
