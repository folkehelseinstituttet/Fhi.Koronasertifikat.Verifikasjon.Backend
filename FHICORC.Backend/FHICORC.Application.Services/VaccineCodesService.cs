using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FHICORC.Application.Models.SmartHealthCard;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Application.Services.Interfaces;
using FHICORC.Domain.Models;
using Microsoft.Extensions.Logging;

namespace FHICORC.Application.Services
{
    public class VaccineCodesService : IVaccineCodesService
    {
        private readonly ILogger<TrustedIssuerService> _logger;
        private readonly IVaccineCodesRepository _vaccineCodesRepository;


        public VaccineCodesService(
            ILogger<TrustedIssuerService> logger,
            IVaccineCodesRepository vaccineCodesRepository) 
        {
            _logger = logger;
            _vaccineCodesRepository = vaccineCodesRepository;
        }


        public async Task ReplaceAutomaticVaccines(
            IEnumerable<VaccineCodesModel> vaccineCodesList,
            string codingSystem)
        {
            await _vaccineCodesRepository.ReplaceAutomaticVaccines(vaccineCodesList, codingSystem);
        }

        public async Task<VaccineCodesModel> GetVaccinationInfo(ShcCodeRequestDto shcRequestList)
        {
            try
            {
                foreach (var shcRequest in shcRequestList.Codes)
                {
                     VaccineCodesModel ret = await _vaccineCodesRepository.GetVaccInfo(
                         new VaccineCodeKey() { VaccineCode = shcRequest.Code, CodingSystem = shcRequest.System });

                     if (ret != null)
                         return ret;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Get Issuer" + ex.Message);
                return null;
            }
        }

        public async Task<bool> RemoveAllVaccineCodes(bool onlyAuto = false)
        {
            try
            {
                var res = await _vaccineCodesRepository.CleanTable(onlyAuto);
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError("Remove all codes" + ex.Message);
                return false;
            }
        }

        public async Task<bool> RemoveVaccineCode(VaccineCodeKey vaccineCodeKey)
        {
            try
            {
                var res = await _vaccineCodesRepository.RemoveVaccineCode(vaccineCodeKey);
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError("Remove Issuer" + ex.Message);
                return false;
            }
        }

        public async Task AddVaccineCode(VaccineCodesDto vaccineCodesModel , bool addedManually)
        {
            IEnumerable<VaccineCodesModel> vaccineCodesModels = vaccineCodesModel.Codes.Select(x => new VaccineCodesModel()
            {
                VaccineCode = x.Code,
                CodingSystem = x.System,
                Name = x.Name,
                Manufacturer = x.Manufacturer,
                Target = x.Target,
                Type = x.Type,
                IsAddManually = addedManually
            });
            await _vaccineCodesRepository.AddVaccineCode(vaccineCodesModels);
        }
    }
}
