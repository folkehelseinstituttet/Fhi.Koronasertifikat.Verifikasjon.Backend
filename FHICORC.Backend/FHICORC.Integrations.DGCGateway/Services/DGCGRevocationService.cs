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

namespace FHICORC.Integrations.DGCGateway.Services
{
    public class DGCGRevocationService : IDGCGRevocationService
    {
        private readonly ILogger<DGCGRevocationService> _logger;
        private readonly CoronapassContext _coronapassContext;
        private readonly IDgcgService _dgcgService;

        public DGCGRevocationService(ILogger<DGCGRevocationService> logger, CoronapassContext coronapassContext, IDgcgService dgcgService)
        {
            _logger = logger;
            _coronapassContext = coronapassContext;
            _dgcgService = dgcgService;
        }

        public async void PopulateRevocationDatabase(DgcgRevocationBatchListRespondDto revocationBatchList) {
            foreach (var rb in revocationBatchList.Batches)
            {
                try
                {
                    var revocationHashList = await _dgcgService.GetRevocationBatchAsync(rb.BatchId);
                    AddToDatabase(rb, revocationHashList);
                }
                catch (Exception e) { }
            }
        }

        public void AddToDatabase(DgcgRevocationListBatchItem batchRoot, DGCGRevocationBatchRespondDto batch) {

            var batchId = batchRoot.BatchId;
            var revocationBatch = FillInBatchRevoc(batchRoot, batch);

            var filter = GenerateBatchFilter(batch.Entries, 47936, 32);

            var filterBytes = BloomFilterUtils.BitToByteArray(filter);
            var filtersRevoc = FillInFilterRevoc(batchId, filterBytes);
                
            var superId = FillInSuperFilterRevoc(batch.Entries.Count, filterBytes);
            revocationBatch.SuperId = superId;

            _coronapassContext.RevocationBatch.Add(revocationBatch);
            _coronapassContext.FiltersRevoc.Add(filtersRevoc);
            AddHashRevoc(batchId, batch);

            _coronapassContext.SaveChanges();
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

        public static BitArray GenerateBatchFilter(List<DgcgHashItem> dgcgHashItems, int m, int k) {
            var filter = new BitArray(m);

            foreach (var h in dgcgHashItems)
            {
                filter.AddToFilter(h.Hash, m, k);
            }
            return filter;
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

        public static FiltersRevoc FillInFilterRevoc(string batchId, byte[] filterBytes) {
            var filtersRevoc = new FiltersRevoc()
            {
                BatchId = batchId,
                Filter = filterBytes,
            };

            return filtersRevoc;    
        }

        public int FillInSuperFilterRevoc(int currenBatchCount, byte[] filterBytes){               

            foreach (var su in _coronapassContext.SuperFiltersRevoc)
            {
                if (su.BatchCount + currenBatchCount <= 1000)
                {
                    var _newbatchFilter = new BitArray(filterBytes);
                    var _oldBatchFilter = new BitArray(su.SuperFilter);

                    var combinedFilter = _newbatchFilter.Or(_oldBatchFilter);
                    var combinedFilterBytes = BloomFilterUtils.BitToByteArray(combinedFilter);

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

                RestructureSuperFilters(superBatchIdsToRecalculate.ToList());

            }
            catch (Exception ex)
            {

            }
        }

        public void RestructureSuperFilters(List<int> superIds)
        {
            foreach (var id in superIds)
            {
                var m = 47936;
                var filter = new BitArray(m);
                var batchCount = 0;

                var superBatch = _coronapassContext.SuperFiltersRevoc
                    .Include(r => r.RevocationBatches)
                        .ThenInclude(x => x.FiltersRevoc)
                    .Include(r => r.RevocationBatches)
                        .ThenInclude(x => x.RevocationHashes)
                    .FirstOrDefault(b => b.Id == id);

                superBatch.RevocationBatches
                    .Where(b => !b.Deleted)
                    .ToList()
                    .ForEach(f => {
                        filter.Or(new BitArray(f.FiltersRevoc.Filter));
                        batchCount += f.RevocationHashes.Count;
                    });

                var filterByte = BloomFilterUtils.BitToByteArray(filter);
                superBatch.SuperFilter = filterByte;
                superBatch.BatchCount = batchCount;
                superBatch.Modified = DateTime.Now;

                _coronapassContext.Entry(superBatch).State = EntityState.Modified;

            }
            _coronapassContext.SaveChanges();
        }
    }

}
