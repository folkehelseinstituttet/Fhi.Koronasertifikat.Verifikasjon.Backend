using System.Collections;
using System;
using Microsoft.Extensions.Logging;
using FHICORC.Domain.Models;
using FHICORC.Application.Models;
using FHICORC.Infrastructure.Database.Context;
using FHICORC.Integrations.DGCGateway.Util;
using Microsoft.EntityFrameworkCore;

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

        public void AddToDatabase(DgcgRevocationListBatchItem batchRoot, DGCGRevocationBatchRespondDto batch) {

            var batchId = batchRoot.BatchId;
            var batchesRevoc = FillInBatchRevoc(batchRoot, batch);

            var filter = GenerateBatchFilter(batch, 47936, 32);

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


        public static BatchesRevoc FillInBatchRevoc(DgcgRevocationListBatchItem batchRoot, DGCGRevocationBatchRespondDto batch) {
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

        public static BitArray GenerateBatchFilter(DGCGRevocationBatchRespondDto batch, int m, int k) {
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

        public static FiltersRevoc FillInFilterRevoc(string batchId, byte[] filterBytes) {
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
