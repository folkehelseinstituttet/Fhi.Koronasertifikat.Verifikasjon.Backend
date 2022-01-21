using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FHICORC.Application.Models;
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

       
        public async Task<VaccineCodesModel> GetVaccInfo (VaccineCodeKey vck)
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

        public async Task<bool> UpdatevaccineCodesList(List<VaccineCodesModel> vaccineCodesList)
        {
            await CleanTable(false);

            await using var transaction = await _coronapassContext.Database.BeginTransactionAsync();
            try
            {
                _coronapassContext.VaccineCodesModels.AddRange(vaccineCodesList);
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e, "Failed Update vaccine code List");
                return false;
            }
        }

        public async Task<bool> CleanTable(bool onlyAuto = true)
        {
            try
            {
                IQueryable<VaccineCodesModel> all;
                all = onlyAuto ? _coronapassContext.VaccineCodesModels.Where(c => c.IsAddManually == false) : _coronapassContext.VaccineCodesModels;

                _coronapassContext.VaccineCodesModels.RemoveRange(all);
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

       

        public async Task AddVaccineCode(IEnumerable<VaccineCodesModel> vaccineCodesModel)
        {
            _coronapassContext.VaccineCodesModels.AddRange(vaccineCodesModel);
            await _coronapassContext.SaveChangesAsync();
        }
    }
}
