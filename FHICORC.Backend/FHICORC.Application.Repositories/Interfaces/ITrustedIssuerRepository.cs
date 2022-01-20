using System.Collections.Generic;
using System.Threading.Tasks;
using FHICORC.Domain.Models;

namespace FHICORC.Application.Repositories.Interfaces
{
    public interface ITrustedIssuerRepository
    {
        Task<TrustedIssuerModel> GetIssuer(string iss);
        Task<bool> UpdateIssuerList(List<TrustedIssuerModel> trustedIssuerList);
        Task<bool> CleanTable();
        Task<bool> RemoveIssuer(string iss);
        Task<string> AddIssuer(TrustedIssuerModel trustedIssuerModel);
    }
}