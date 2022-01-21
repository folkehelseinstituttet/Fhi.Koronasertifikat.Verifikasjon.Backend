using System.Collections.Generic;
using System.Threading.Tasks;
using FHICORC.Application.Models;
using FHICORC.Application.Models.SmartHealthCard;
using FHICORC.Domain.Models;

namespace FHICORC.Application.Services.Interfaces
{
    public interface IVaccineCodesService
    {
        public Task<VaccineCodesModel> GetVaccinationInfo(ShcCodeRequestDto shcRequestList);
        Task UpdateVaccineCodesList(List<VaccineCodesModel> vaccineCodesList);
        Task<bool> RemoveAllVaccineCodes(bool onlyAuto = false);
        Task<bool> RemoveVaccineCode(VaccineCodeKey vaccineCodeKey);
        Task AddVaccineCode(VaccineCodesDto vaccineCodesModel, bool addedManually);
    }
}