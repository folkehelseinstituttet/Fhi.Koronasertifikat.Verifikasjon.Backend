﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Domain.Models;
using FHICORC.Infrastructure.Database.Context;
using Microsoft.Extensions.Logging;
using Npgsql.Bulk;

namespace FHICORC.Application.Repositories
{
    public class TrustedIssuerRepository : ITrustedIssuerRepository
    {
        private readonly CoronapassContext _coronapassContext;
        private readonly ILogger<TrustedIssuerRepository> _logger;
        private readonly NpgsqlBulkUploader _bulkUploader;

        public TrustedIssuerRepository(CoronapassContext coronapassContext, ILogger<TrustedIssuerRepository> logger)
        {
            _coronapassContext = coronapassContext;
            _logger = logger;
            _bulkUploader = new NpgsqlBulkUploader(_coronapassContext);
        }

        public TrustedIssuerModel GetIssuer(string iss)
        {
            try
            {
                return _coronapassContext.TrustedIssuerModels.SingleOrDefault(p => p.Iss == iss);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed Get issuer" + e.Message);
                return null;
            }
        }

        public async Task AddIssuer(TrustedIssuerModel trustedIssuerModel)
        {
            _coronapassContext.TrustedIssuerModels.Add(trustedIssuerModel);
            await _coronapassContext.SaveChangesAsync();
        }

        public async Task AddIssuers(IEnumerable<TrustedIssuerModel> trustedIssuerList)
        {
            _coronapassContext.TrustedIssuerModels.AddRange(trustedIssuerList);
            await _coronapassContext.SaveChangesAsync();
        }

        public async Task ReplaceAutomaticallyAddedIssuers(IEnumerable<TrustedIssuerModel> trustedIssuerList)
        {
            await using var transaction = await _coronapassContext.Database.BeginTransactionAsync();
            try
            {
                await CleanTable(true);
                _bulkUploader.Insert(trustedIssuerList,
                    InsertConflictAction.UpdateProperty<TrustedIssuerModel>(x => x.Iss, x => x.Name));
                transaction.Commit();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e, "Failed to clean TRusted issuer Table");
                throw;
            }
        }

        public async Task<bool> CleanTable(bool keepIsAddManually)
        {
            try
            {
                var range = keepIsAddManually ? _coronapassContext.TrustedIssuerModels.Where(x => !x.IsAddManually) : _coronapassContext.TrustedIssuerModels;
                _coronapassContext.TrustedIssuerModels.RemoveRange(range);
                await _coronapassContext.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to clean TRusted issuer Table");
                return false;
            }
        }

        public async Task<bool> RemoveIssuer(string iss)
        {
            try
            {
                var res = _coronapassContext.TrustedIssuerModels.SingleOrDefault(p => p.Iss == iss);
                _coronapassContext.TrustedIssuerModels.Remove(res);
                await _coronapassContext.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed Get issuer" + e.Message);
                return false;
            }
        }
    }
}
