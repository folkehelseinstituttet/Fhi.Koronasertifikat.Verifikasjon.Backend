using System.Collections.Generic;
using System.Threading.Tasks;
using FHICORC.Application.Models.SmartHealthCard;
using FHICORC.Domain.Models;

namespace FHICORC.Application.Services.Interfaces
{
    public interface IVaccineCodesService
    {
        Task ReplaceAutomaticVaccines(
            IEnumerable<VaccineCodesModel> vaccineCodesList,
            string codingSystem);
        Task<VaccineCodesModel> GetVaccinationInfo(ShcCodeRequestDto shcRequestList);
        Task AddVaccineCode(VaccineCodesDto vaccineCodesModel, bool addedManually);
        Task<bool> RemoveAllVaccineCodes(bool onlyAuto = false);
        Task<bool> RemoveVaccineCode(VaccineCodeKey vaccineCodeKey);
    }
}