﻿using FHICORC.Application.Models;
using FHICORC.Infrastructure.Database.Context;
using FHICORC.Integrations.DGCGateway.Util;
using System.Collections.Generic;
using FHICORC.Core.Services.Enum;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using FHICORC.Application.Models.Revocation;

namespace FHICORC.Application.Services
{
    public class RevocationFetchService : IRevocationFetchService
    {
        private readonly ILogger<RevocationFetchService> _logger;
        private readonly CoronapassContext _coronapassContext;

        public RevocationFetchService(ILogger<RevocationFetchService> logger, CoronapassContext coronapassContext)
        {
            _logger = logger;
            _coronapassContext = coronapassContext;
        }

        public IEnumerable<SuperBatch> FetchSuperBatches(DateTime dateTime)
        {
            try
            {
                var superBatchList = _coronapassContext.RevocationSuperFilter
                    .Where(s => s.Modified >= dateTime)
                    .Select(x => new SuperBatch(x.Id, x.SuperCountry, x.Bucket, x.SuperFilter, (HashTypeEnum)x.HashType, x.SuperExpires));

                return superBatchList;
            }
            catch (Exception e)
            {

                _logger.LogError("Unable to fetch SuperBatches for last modified date {dateTime}", dateTime);
                return null;
            }
        }

        public IEnumerable<int> FetchSuperBatchRevocationList(DateTime dateTime)
        {
            try
            {
                var superBatchList = _coronapassContext.RevocationSuperFilter
                    .Where(s => s.Modified >= dateTime)
                    .Select(x => x.Id);

                return superBatchList;
            }
            catch (Exception e)
            {

                _logger.LogError("Unable to fetch SuperBatches for last modified date {dateTime}", dateTime);
                return null;
            }
        }

        public SuperBatch FetchSuperBatch(int id)
        {
            try
            {
                var superBatch = _coronapassContext.RevocationSuperFilter
                    .Where(s => s.Id == id)
                    .Select(x => new SuperBatch(x.Id, x.SuperCountry, x.Bucket, x.SuperFilter, (HashTypeEnum)x.HashType, x.SuperExpires))
                    .FirstOrDefault();

                return superBatch;

            }

            catch (Exception e)
            {
                _logger.LogError("Unable to fetch SuperBatche for id {id}: {e}", id, e);
                return null;
            }

        }

        public SuperBatchChunkDto FetchSuperBatchesChunk(DateTime dateTime)
        {
            var amountOfBatchesToTransfer = 10;
            DateTime modifiedOfLastSuperBatch = default;

            try
            {
                var revocationSuperFilterList = _coronapassContext.RevocationSuperFilter
                    .OrderBy(s => s.Modified)
                    .Where(s => s.Modified >= dateTime)
                    .Take(amountOfBatchesToTransfer + 1);

                var superBatchList = revocationSuperFilterList
                    .Select(x => new SuperBatch(x.Id, x.SuperCountry, x.Bucket, x.SuperFilter, (HashTypeEnum)x.HashType, x.SuperExpires));

                if (revocationSuperFilterList.Count() > amountOfBatchesToTransfer)
                {
                    modifiedOfLastSuperBatch = revocationSuperFilterList.Last().Modified;
                    return new SuperBatchChunkDto(modifiedOfLastSuperBatch, superBatchList.Take(amountOfBatchesToTransfer), true);
                }

                return new SuperBatchChunkDto(modifiedOfLastSuperBatch, superBatchList);
            }
            catch (Exception e)
            {
                _logger.LogError("Error: {e}", e);
            }
            return null;

        }

    }
}
