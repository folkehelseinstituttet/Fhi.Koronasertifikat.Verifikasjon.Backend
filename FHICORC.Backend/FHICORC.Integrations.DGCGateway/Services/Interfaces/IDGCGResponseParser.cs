using FHICORC.Application.Models;
using FHICORC.Domain.Models;
using System.Collections.Generic;

namespace FHICORC.Integrations.DGCGateway.Services.Interfaces
{
    public interface IDgcgResponseParser
    {
        public List<EuDocSignerCertificate> ParseToEuDocSignerCertificate(DgcgTrustListResponseDto dgcgTrustListResponseDto);
    }
}
