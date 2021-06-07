using FHICORC.Application.Models;

namespace FHICORC.Integrations.DGCGateway.Services.Interfaces
{
    public interface IDgcgResponseVerification
    {
        public DgcgTrustListResponseDto VerifyResponseFromGateway(DgcgTrustListResponseDto gatewayResponse); 
    }
}
