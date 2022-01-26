using System.Threading.Tasks;
using FHICORC.Application.Models.SmartHealthCard;
using FHICORC.Domain.Models;

namespace FHICORC.Application.Services.Interfaces
{
    public interface ITrustedIssuerService
    {
        public TrustedIssuerModel GetIssuer(string iss);
        public Task AddIssuers(AddIssuersRequest issuers, bool isAddManually);
        public Task ReplaceAutomaticallyAddedIssuers(ShcIssuersDto issuers);
        public bool UpdateIsTrusted(string iss, bool trusted);
        public Task<bool> RemoveIssuer(string iss);
        public Task<bool> RemoveAllIssuers(bool keepIsAddManually = false);
    }
}