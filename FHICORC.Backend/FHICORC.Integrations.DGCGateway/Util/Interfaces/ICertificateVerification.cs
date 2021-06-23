using System.Collections.Generic;
using FHICORC.Application.Models;

namespace FHICORC.Integrations.DGCGateway.Util.Interfaces
{
    public interface ICertificateVerification<TCert>
    {
        public bool VerifyDscSignedByCsca(DgcgTrustListItem trustListItem, List<TCert> cscaCertificates);

        public bool VerifyItemByAnchorSignature(DgcgTrustListItem trustListItem, TCert anchorCertificate, string anchorCertificateType);
        public bool VerifyItemByAnchorSignature(DgcgTrustListItem trustListItem, List<TCert> anchorCertificates, string anchorCertificateType);
    }
}
