using System.Threading.Tasks;
using FHICORC.Application.Models;

namespace FHICORC.Application.Services.Interfaces
{
    public interface ISHCService
    {
        //public Task<RuleResponseDto> GetRulesAsync();
        public Task<ShcVaccineResponseDto> GetVaccinationInfosync(ShcRequestDto shcRequest);
        public Task<ShcTrustResponseDto> GetIsTrustedsync(ShcTrustRequestDto shcRequestDeserialized);
    }
}