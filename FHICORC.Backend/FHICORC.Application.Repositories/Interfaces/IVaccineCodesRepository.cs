using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FHICORC.Application.Models;
using FHICORC.Domain.Models;

namespace FHICORC.Application.Repositories.Interfaces
{
    public interface IVaccineCodesRepository
    {
        Task<VaccineCodesModel> GetVaccInfo(VaccineCodeKey vaccineCodeKey);
        Task<bool> UpdatevaccineCodesList(List<VaccineCodesModel> vaccineCodesList);
        Task<bool> CleanTable(bool onlyAuto = true);
        Task<bool> RemoveVaccineCode(VaccineCodeKey vaccineCodeKey);
        Task AddVaccineCode(IEnumerable<VaccineCodesModel> vaccineCodesModel);
    }
}