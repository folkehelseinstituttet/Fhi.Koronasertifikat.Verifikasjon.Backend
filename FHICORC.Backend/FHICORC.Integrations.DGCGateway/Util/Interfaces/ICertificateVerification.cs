using FHICORC.Application.Models;
using System.Security.Cryptography.X509Certificates;


namespace FHICORC.Integrations.DGCGateway.Util.Interfaces
{
    public interface ICertificateVerification
    {
        public bool VerifyDSCSignedByCSCA(DgcgTrustListItem TrustListItem, X509Certificate2 CSCACertificate);

        public bool VerifyByTrustAnchorSignature(DgcgTrustListItem trustListItem, X509Certificate2 TrustAnchorCertificate);

        public bool VerifyDSCByUploadCertificate(DgcgTrustListItem TrustListItemDSC, X509Certificate2 UploadCertificate);
    }



        
    
}
