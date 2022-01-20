using System.Collections.Generic;
using System.Threading.Tasks;
using FHICORC.Domain.Models;

namespace FHICORC.Application.Repositories.Interfaces
{
    public interface ITrustedIssuerRepository
    {
        TrustedIssuerModel GetIssuer(string iss);
        Task AddIssuer(TrustedIssuerModel trustedIssuerModel);
        Task AddIssuers(IEnumerable<TrustedIssuerModel> trustedIssuerList);
        Task ReplaceAutomaticallyAddedIssuers(IEnumerable<TrustedIssuerModel> trustedIssuerList);
        Task<bool> CleanTable(bool keepIsAddManually);
        Task<bool> RemoveIssuer(string iss);
    }
}