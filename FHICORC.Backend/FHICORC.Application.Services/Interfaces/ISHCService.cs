using System.Threading.Tasks;
using FHICORC.Application.Models;

namespace FHICORC.Application.Services.Interfaces
{
    public interface ISHCService
    {
        public Task<ShcVaccineResponseDto> GetVaccinationInfosync(ShcCodeRequestDto shcRequest);
        public Task<ShcTrustResponseDto> GetIsTrustedsync(ShcTrustRequestDto shcRequestDeserialized);
    }
}