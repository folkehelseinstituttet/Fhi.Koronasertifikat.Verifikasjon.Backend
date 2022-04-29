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

namespace FHICORC.Integrations.DGCGateway.Services
{
    public class DGCGRevocationService : IDGCGRevocationService
    {
        private readonly ILogger<DGCGRevocationService> _logger;
        private readonly CoronapassContext _coronapassContext;

        public DGCGRevocationService(ILogger<DGCGRevocationService> logger, CoronapassContext coronapassContext)
        {
            _logger = logger;
            _coronapassContext = coronapassContext;
        }

        public void DeleteExpiredBatches()
        {
            var batchesToDelete = _coronapassContext.BatchesRevoc
                .Where(b => !b.Deleted && b.Expires <= DateTime.UtcNow);

            var superBatchIdsToRecalculate = new HashSet<int>();
            foreach (var b in batchesToDelete)
            {
                b.Deleted = true;

                if (b.SuperId is not null)
                    superBatchIdsToRecalculate.Add((int)b.SuperId);

                b.SuperId = null;
            }

            _coronapassContext.Entry(batchesToDelete).State = EntityState.Modified;
            _coronapassContext.SaveChanges();

            RestructureSuperFilters(superBatchIdsToRecalculate.ToList());

        }

        public void RestructureSuperFilters(List<int> superIds)
        {

            foreach (var id in superIds)
            {
                var m = 47936;
                var filter = new BitArray(m);
                var cnt = 0;

                var superBatch = _coronapassContext.SuperFiltersRevoc.FirstOrDefault(b => b.Id == id);

                _ = superBatch.BatchesRevocs
                    .Where(b => !b.Deleted)
                    .Select(f => new
                    {
                        _ = filter.Or(new BitArray(f.FiltersRevoc.Filter)),
                        c = cnt += f.HashesRevocs.Count
                    });
                    
                var filterByte = BloomFilterUtils.BitToByteArray(filter);
                superBatch.SuperFilter = filterByte;
                superBatch.BatchCount = cnt;

                _coronapassContext.Entry(superBatch).State = EntityState.Modified;

            }
            _coronapassContext.SaveChanges();

        }

        public void AddToDatabase(DgcgRevocationListBatchItem batchRoot, DGCGRevocationBatchRespondDto batch) {

            var batchId = batchRoot.BatchId;
            var batchesRevoc = FillInBatchRevoc(batchRoot, batch);

            var filter = GenerateBatchFilter(batch);
            byte[] filterBytes = new byte[(filter.Length - 1) / 8 + 1];
            filter.CopyTo(filterBytes, 0);
            var filtersRevoc = FillInFilterRevoc(batchId, filterBytes);
                
            var superId = FillInSuperFilterRevoc(batch, filterBytes);
            batchesRevoc.SuperId = superId;

            _coronapassContext.BatchesRevoc.Add(batchesRevoc);
            _coronapassContext.FiltersRevoc.Add(filtersRevoc);
            AddHashRevoc(batchId, batch);

            _coronapassContext.SaveChanges();
        }


        private BatchesRevoc FillInBatchRevoc(DgcgRevocationListBatchItem batchRoot, DGCGRevocationBatchRespondDto batch) {
            var batchesRevoc = new BatchesRevoc()
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
            return batchesRevoc;
        }

        private BitArray GenerateBatchFilter(DGCGRevocationBatchRespondDto batch) {
            var m = 47936;
            var k = 32;

            var filter = new BitArray(m);

            foreach (var b in batch.Entries)
            {
                filter.AddToFilter(b.Hash, m, k);
            }
            return filter;
        }

        private void AddHashRevoc(string batchId, DGCGRevocationBatchRespondDto batch) {
            foreach (var b in batch.Entries)
            {
                var _hashesRevoc = new HashesRevoc()
                {
                    BatchId = batchId,
                    Hash = b.Hash
                };
                _coronapassContext.HashesRevoc.Add(_hashesRevoc);
            }
        }

        private FiltersRevoc FillInFilterRevoc(string batchId, byte[] filterBytes) {
            var filtersRevoc = new FiltersRevoc()
            {
                BatchId = batchId,
                Filter = filterBytes,
            };

            return filtersRevoc;    
        }

        private int FillInSuperFilterRevoc(DGCGRevocationBatchRespondDto batch, byte[] filterBytes) {
            var currenBatchCount = batch.Entries.Count;

            foreach (var su in _coronapassContext.SuperFiltersRevoc)
            {
                if (su.BatchCount + currenBatchCount <= 1000)
                {
                    var _newbatchFilter = new BitArray(filterBytes);
                    var _oldBatchFilter = new BitArray(su.SuperFilter);

                    var combinedFilter = _newbatchFilter.Or(_oldBatchFilter);
                    var combinedFilterBytes = new byte[(combinedFilter.Length - 1) / 8 + 1];
                    combinedFilter.CopyTo(combinedFilterBytes, 0);

                    su.SuperFilter = combinedFilterBytes;
                    su.BatchCount += currenBatchCount;
                    su.Modified = DateTime.UtcNow;

                    _coronapassContext.Entry(su).State = EntityState.Modified;

                    return su.Id;

                }
            }

            var superFilterRevoc = new SuperFiltersRevoc()
            {
                SuperFilter = filterBytes,
                BatchCount = currenBatchCount,
                Modified = DateTime.UtcNow
            };

            _coronapassContext.SuperFiltersRevoc.Add(superFilterRevoc);
            _coronapassContext.SaveChanges();
            return superFilterRevoc.Id;


        }
    }

}
