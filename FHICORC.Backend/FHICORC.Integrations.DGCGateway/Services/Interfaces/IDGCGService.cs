using System.Threading.Tasks;
using FHICORC.Application.Models;

namespace FHICORC.Integrations.DGCGateway.Services.Interfaces
{
    public interface IDgcgService
    {
        public Task<DgcgTrustListResponseDto> GetTrustListAsync(string certificateType = "");
    }
}
