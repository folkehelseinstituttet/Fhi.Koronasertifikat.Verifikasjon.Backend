using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Domain.Models;
using FHICORC.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FHICORC.Application.Repositories
{
    public class TrustedIssuerRepository : ITrustedIssuerRepository
    {
        private readonly CoronapassContext _coronapassContext;
        private readonly ILogger<TrustedIssuerRepository> _logger;

        public TrustedIssuerRepository(CoronapassContext coronapassContext, ILogger<TrustedIssuerRepository> logger)
        {
            _coronapassContext = coronapassContext;
            _logger = logger;
        }

       
        public async Task<TrustedIssuerModel> GetIssuer(string iss)
        {
            await using var transaction = await _coronapassContext.Database.BeginTransactionAsync();
            try
            {
                var res = _coronapassContext.TrustedIssuerModels.SingleOrDefault(p => p.Iss == iss);
                return res;
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e, "Failed Get issuer" + e.Message);
                return null;
            }
        }

        public async Task<bool> UpdateIssuerList(List<TrustedIssuerModel> trustedIssuerList)
        {
            await CleanTable();

            await using var transaction = await _coronapassContext.Database.BeginTransactionAsync();
            try
            {
                //var trustedIssuerTableName = _coronapassContext.Model.FindEntityType(typeof(TrustedIssuerModel)).GetTableName();
                //var rowsAffected = _coronapassContext.Database.ExecuteSqlRaw($"insert into \"{trustedIssuerTableName}\" values ('{trustedIssuerModel.Iss}' , '{trustedIssuerModel.Name}','{true}')");

                _coronapassContext.TrustedIssuerModels.AddRange(trustedIssuerList);

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e, "Failed Update Trusted Issuer List");
                return false;
            }
        }

        public async Task<bool> CleanTable()
        {
            await using var transaction = await _coronapassContext.Database.BeginTransactionAsync();
            try
            {
                //var countriesReportTableName = _coronapassContext.Model.FindEntityType(typeof(CountrioesReportModel)).GetTableName();
                //var rowsAffected = _coronapassContext.Database.ExecuteSqlRaw($"delete from \"{countriesReportTableName}\"");
                var all = from c in _coronapassContext.TrustedIssuerModels select c;
                _coronapassContext.TrustedIssuerModels.RemoveRange(all);
                _coronapassContext.SaveChanges();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e, "Failed to clean TRusted issuer Table");
                return false;
            }
        }

        public async Task<bool> RemoveIssuer(string iss)
        {
            await using var transaction = await _coronapassContext.Database.BeginTransactionAsync();
            try
            {
                var res = _coronapassContext.TrustedIssuerModels.SingleOrDefault(p => p.Iss == iss);
                _coronapassContext.TrustedIssuerModels.Remove(res);
                _coronapassContext.SaveChanges();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e, "Failed Get issuer" + e.Message);
                return false;
            }
        }

        public async Task<string> AddIssuer(TrustedIssuerModel trustedIssuerModel)
        {
            await using var transaction = await _coronapassContext.Database.BeginTransactionAsync();
            try
            {
                var trustedIssuerTableName = _coronapassContext.Model.FindEntityType(typeof(TrustedIssuerModel)).GetTableName();
                var rowsAffected = _coronapassContext.Database.ExecuteSqlRaw($"insert into \"{trustedIssuerTableName}\" values ('{trustedIssuerModel.Iss}' , '{trustedIssuerModel.Name}','{true}')");

                //_coronapassContext.TrustedIssuerModels.Add(trustedIssuerModel);

                await transaction.CommitAsync();
                return "Issuer added";
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e, "Failed insert statement trusted issuer. " + e.Message );
                return e.Message;
            }
        }
    }
}
