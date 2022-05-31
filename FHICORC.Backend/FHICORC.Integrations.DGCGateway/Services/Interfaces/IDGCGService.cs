﻿using System.Threading.Tasks;
using FHICORC.Application.Models;

namespace FHICORC.Integrations.DGCGateway.Services.Interfaces
{
    public interface IDgcgService
    {
        public Task<DgcgTrustListResponseDto> GetTrustListAsync(string certificateType = "");
        public Task<DgcgRevocationBatchListRespondDto> GetRevocationBatchListAsync(string modifiedSince);
        public Task<DGCGRevocationBatchRespondDto> GetRevocationBatchAsync(string batchId);
    }
}
