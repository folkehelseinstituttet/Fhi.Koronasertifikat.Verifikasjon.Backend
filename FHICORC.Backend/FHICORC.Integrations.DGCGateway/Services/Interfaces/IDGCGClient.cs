using FHICORC.Application.Models;
using System.Threading.Tasks;

namespace FHICORC.Integrations.DGCGateway.Services.Interfaces
{
    public interface IDgcgClient
    {
        public Task<DgcgTrustListResponseDto> FetchTrustListAsync(string certificateType);
    }
}
