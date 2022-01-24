using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FHICORC.Application.Models.SmartHealthCard;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Domain.Models;
using FHICORC.Infrastructure.Database.Context;
using Microsoft.Extensions.Logging;

namespace FHICORC.Application.Repositories
{
    public class VaccineCodesRepository : IVaccineCodesRepository
    {
        private readonly CoronapassContext _coronapassContext;
        private readonly ILogger<VaccineCodesRepository> _logger;

        public VaccineCodesRepository(CoronapassContext coronapassContext, ILogger<VaccineCodesRepository> logger)
        {
            _coronapassContext = coronapassContext;
            _logger = logger;
        }

        public async Task ReplaceAutomaticVaccines(
            IEnumerable<VaccineCodesModel> vaccineCodesList,
            string codingSystem)
        {
            await using var transaction = _coronapassContext.Database.BeginTransaction();
            try
            {
                _coronapassContext.VaccineCodesModels.RemoveRange(_coronapassContext.VaccineCodesModels
                    .Where(x => x.CodingSystem.Equals(codingSystem) && x.IsAddManually));
                _coronapassContext.SaveChanges();
                _coronapassContext.VaccineCodesModels.AddRange(vaccineCodesList);
                _coronapassContext.SaveChanges();
                transaction.Commit();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                _logger.LogError(e, "Failed Update vaccine code List");
                throw;
            }
        }
       
        public async Task<VaccineCodesModel> GetVaccInfo(VaccineCodeKey vck)
        {
            await using var transaction = await _coronapassContext.Database.BeginTransactionAsync();
            try
            {
                var res = _coronapassContext.VaccineCodesModels.SingleOrDefault(p => p.VaccineCode == vck.VaccineCode && p.CodingSystem == vck.CodingSystem);
                return res;
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e, "Failed vaccine Info" + e.Message);
                return null;
            }
        }

        public async Task AddVaccineCode(IEnumerable<VaccineCodesModel> vaccineCodesModel)
        {
            _coronapassContext.VaccineCodesModels.AddRange(vaccineCodesModel);
            _coronapassContext.SaveChanges();
            await _coronapassContext.SaveChangesAsync();
        }

        public async Task<bool> CleanTable(bool onlyAuto = true)
        {
            try
            {
                IQueryable<VaccineCodesModel> all;
                all = onlyAuto ? _coronapassContext.VaccineCodesModels.Where(c => c.IsAddManually == false) : _coronapassContext.VaccineCodesModels;

                _coronapassContext.VaccineCodesModels.RemoveRange(all);
                _coronapassContext.SaveChanges();
                await _coronapassContext.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to clean vaccine codes Table");
                return false;
            }
        }

        public async Task<bool> RemoveVaccineCode(VaccineCodeKey vck)
        {
            await using var transaction = await _coronapassContext.Database.BeginTransactionAsync();
            try
            {
                var res = _coronapassContext.VaccineCodesModels.SingleOrDefault(p => p.VaccineCode == vck.VaccineCode && p.CodingSystem == vck.CodingSystem);
                _coronapassContext.VaccineCodesModels.Remove(res ?? throw new InvalidOperationException());
                _coronapassContext.SaveChanges();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e, "Failed Get vaccine code" + e.Message);
                return false;
            }
        }
    }
}
