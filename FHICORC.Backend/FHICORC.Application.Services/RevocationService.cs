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
using FHICORC.Application.Models;

namespace FHICORC.Application.Services
{
    public class RevocationService : IRevocationService
    {
        private readonly ILogger<RevocationService> _logger;
        private readonly CoronapassContext _coronapassContext;

        public RevocationService(ILogger<RevocationService> logger, CoronapassContext coronapassContext)
        {
            _logger = logger;
            _coronapassContext = coronapassContext;
        }

        public void AddToDatabase(DgcgRevocationListBatchItem batchRoot, DGCGRevocationBatchRespondDto batch) {

            FillInBatchRevoc(batchRoot, batch);

            var filter = FillInHashRevoc(batchRoot, batch);

            byte[] filterBytes = new byte[filter.Length / 8];
            filter.CopyTo(filterBytes, 0);
            FillInFilterRevoc(batchRoot, filterBytes);

            FillInSuperFilterRevoc(batch, filter, filterBytes);

        }


        private void FillInBatchRevoc(DgcgRevocationListBatchItem batchRoot, DGCGRevocationBatchRespondDto batch) {
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
            _coronapassContext.BatchesRevoc.Add(batchesRevoc);
            _coronapassContext.SaveChanges();
        }

        private BitArray FillInHashRevoc(DgcgRevocationListBatchItem batchRoot, DGCGRevocationBatchRespondDto batch) {

            var m = 47936;
            var k = 32;

            var filter = new BitArray(m);

            foreach (var b in batch.Entries)
            {
                var _hashesRevoc = new HashesRevoc()
                {
                    BatchId = batchRoot.BatchId,
                    Hash = b.Hash
                };

                filter.AddToFilter(b.Hash, m, k);
                _coronapassContext.HashesRevoc.Add(_hashesRevoc);
            }
            _coronapassContext.SaveChanges();


            return filter;
        }

        private void FillInFilterRevoc(DgcgRevocationListBatchItem batchRoot, byte[] filterBytes) {
            var filtersRevoc = new FiltersRevoc()
            {
                BatchId = batchRoot.BatchId,
                Filter = filterBytes,
            };
            _coronapassContext.FiltersRevoc.Add(filtersRevoc);
            _coronapassContext.SaveChanges();
        }

        private void FillInSuperFilterRevoc(DGCGRevocationBatchRespondDto batch, BitArray filter, byte[] filterBytes) {
            var exist = false;
            var currenBatchCount = batch.Entries.Count;
            foreach (var su in _coronapassContext.SuperFiltersRevoc)
            {
                if (su.BatchCount + currenBatchCount <= 1000)
                {
                    //1896
                    var _newFilter = new BitArray(su.SuperFilter);
                    
             

                    _newFilter.Or(filter);


                    //var b = _newFilter.Cast<bool>().Select(x => x ? 1 : 0).ToList();
                    //var _tmpb = new List<int>();
                    //for (var i = 0; i < b.Count; i++)
                    //{
                    //    if (b[i] == 1)
                    //        _tmpb.Add(i);

                    //}
                    //var bb = string.Join(",", _tmpb);

                    byte[] _superFilterNewBytes = new byte[_newFilter.Length / 8];
                    filter.CopyTo(_superFilterNewBytes, 0);

                    try
                    {
                        su.SuperFilter = _superFilterNewBytes;
                        su.BatchCount += currenBatchCount;
                        su.Modified = DateTime.UtcNow;

                        _coronapassContext.Entry(su).State = EntityState.Modified;



                        //_coronapassContext.Entry(_superFilterRevoc).State = EntityState.Modified;

                        
                    }
                    catch (Exception ex) { 
                    
                    }


                    exist = true;
                    break;
                }
            }

            if (!exist)
            {
                var superFilterRevoc = new SuperFiltersRevoc()
                {
                    SuperFilter = filterBytes,
                    BatchCount = currenBatchCount,
                    Modified = DateTime.UtcNow
                };

                _coronapassContext.SuperFiltersRevoc.Add(superFilterRevoc);
            }

            _coronapassContext.SaveChanges();


        }
    }
}
