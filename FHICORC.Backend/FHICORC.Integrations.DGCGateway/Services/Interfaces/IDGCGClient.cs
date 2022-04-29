using FHICORC.Application.Models;
using System;
using System.Threading.Tasks;

namespace FHICORC.Integrations.DGCGateway.Services.Interfaces
{
    public interface IDgcgClient
    {
        public Task<DgcgTrustListResponseDto> FetchTrustListAsync(string certificateType);
        public Task<DgcgRevocationBatchListRespondDto> FetchRevocationBatchListAsync(string date);
        public Task<DGCGRevocationBatchRespondDto> FetchRevocationBatchAsync(string batchId);



    }
}
