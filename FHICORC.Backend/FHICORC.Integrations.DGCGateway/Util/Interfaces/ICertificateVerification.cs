using System.Collections.Generic;
using FHICORC.Application.Models;
using System.Security.Cryptography.X509Certificates;

namespace FHICORC.Integrations.DGCGateway.Util.Interfaces
{
    public interface ICertificateVerification
    {
        public bool VerifyDscSignedByCsca(DgcgTrustListItem trustListItem, List<X509Certificate2> cscaCertificates);

        public bool VerifyItemByAnchorSignature(DgcgTrustListItem trustListItem, X509Certificate2 anchorCertificate, string anchorCertificateType);
    }
}
