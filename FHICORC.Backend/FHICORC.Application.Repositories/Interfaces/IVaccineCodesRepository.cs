using System.Collections.Generic;
using System.Threading.Tasks;
using FHICORC.Application.Models.SmartHealthCard;
using FHICORC.Domain.Models;

namespace FHICORC.Application.Repositories.Interfaces
{
    public interface IVaccineCodesRepository
    {
        Task ReplaceAutomaticVaccines(
            IEnumerable<VaccineCodesModel> vaccineCodesList,
            string codingSystem);
        Task<VaccineCodesModel> GetVaccinationInfo(VaccineCodeKey vaccineCodeKey);
        Task<bool> CleanTable(bool onlyAuto = true);
        Task<bool> RemoveVaccineCode(VaccineCodeKey vaccineCodeKey);
        Task AddVaccineCode(IEnumerable<VaccineCodesModel> vaccineCodesModel);
    }
}