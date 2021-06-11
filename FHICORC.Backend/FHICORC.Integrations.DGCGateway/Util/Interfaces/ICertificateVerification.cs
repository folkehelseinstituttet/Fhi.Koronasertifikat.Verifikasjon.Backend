using FHICORC.Application.Models;
using System.Security.Cryptography.X509Certificates;

namespace FHICORC.Integrations.DGCGateway.Util.Interfaces
{
    public interface ICertificateVerification
    {
        public bool VerifyDscSignedByCsca(DgcgTrustListItem trustListItem, X509Certificate2 cscaCertificate);

        public bool VerifyItemByAnchorSignature(DgcgTrustListItem trustListItem, X509Certificate2 anchorCertificate, string anchorCertificateType);
    }
}
