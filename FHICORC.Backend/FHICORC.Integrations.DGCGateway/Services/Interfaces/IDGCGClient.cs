using FHICORC.Application.Models;
using System;
using System.Threading.Tasks;

namespace FHICORC.Integrations.DGCGateway.Services.Interfaces
{
    public interface IDgcgClient
    {
        Task<DgcgTrustListResponseDto> FetchTrustListAsync(string certificateType);
        Task<DgcgRevocationBatchListRespondDto> FetchRevocationBatchListAsync(DateTime date);
        Task<DGCGRevocationBatchRespondDto> FetchRevocationBatchAsync(string batchId);
    }
}
