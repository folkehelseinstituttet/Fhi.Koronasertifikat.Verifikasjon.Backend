using System.Threading.Tasks;
using FHICORC.Application.Models;
using FHICORC.Domain.Models;

namespace FHICORC.Application.Services.Interfaces
{
    public interface ISHCService
    {
        public Task<ShcVaccineResponseDto> GetVaccinationInfosync(ShcCodeRequestDto shcRequest);
        public Task<ShcTrustResponseDto> GetIsTrustedsync(ShcTrustRequestDto shcRequestDeserialized);
        public Task<string> AddIssuer(AddIssuersRequest iss);
        public Task<bool> CleanTable(bool cleanOnlyAuto = true);  // TODO
        public Task<TrustedIssuerModel> GetIssuer(string iss);
        public Task<bool> RemoveIssuer(string iss);
    }
}