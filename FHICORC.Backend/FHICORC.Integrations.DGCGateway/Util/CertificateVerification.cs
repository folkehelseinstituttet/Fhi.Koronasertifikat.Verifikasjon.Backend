using Microsoft.Extensions.Logging;
using FHICORC.Application.Models;
using FHICORC.Integrations.DGCGateway.Util.Interfaces;
using System;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace FHICORC.Integrations.DGCGateway.Util
{
    public class CertificateVerification: ICertificateVerification
    {
        private readonly ILogger<CertificateVerification> _logger;
        private byte[] cmsBytesSignature;
        private byte[] trustListRawData;
        private byte[] dscBytesSignature;
        private byte[] dscTrustListRawData; 

        public CertificateVerification (ILogger<CertificateVerification> logger)
        {
            _logger = logger; 
        }

        public bool VerifyDSCSignedByCSCA(DgcgTrustListItem TrustListItem, X509Certificate2 CSCACertificate)
        {
            var DSCCert = new X509Certificate2(Convert.FromBase64String(TrustListItem.rawData));
            bool result = DSCCert.Issuer.Equals(CSCACertificate.Subject);

            return result;
        }

        public bool VerifyByTrustAnchorSignature(DgcgTrustListItem trustListItem, X509Certificate2 TrustAnchorCertificate)
        {
            try
            {
                cmsBytesSignature = Convert.FromBase64String(trustListItem.signature);
                trustListRawData = Convert.FromBase64String(trustListItem.rawData); 
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Invalid Base64 for signature {kid}", trustListItem.kid);
            }

            try
            {
                ContentInfo content = new ContentInfo(trustListRawData);
                SignedCms signedCms = new SignedCms(content, true);

                //Add Signature to CMS message
                signedCms.Decode(cmsBytesSignature);

                //Check signature validity 
                signedCms.CheckSignature(true);

                var signedCmsCertificate = signedCms.Certificates[0];
                //Verify against Trust Anchor 
                var isCertificateValid = signedCmsCertificate.Equals(TrustAnchorCertificate);

                return isCertificateValid;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to verify certificate type " , trustListItem.certificateType, "with TrustAnchor");
                return false;
            }
        }

        public bool VerifyDSCByUploadCertificate(DgcgTrustListItem TrustListItemDSC, X509Certificate2 UploadCertificate)
        {
            try
            {
                dscBytesSignature = Convert.FromBase64String(TrustListItemDSC.signature);
                dscTrustListRawData = Convert.FromBase64String(TrustListItemDSC.rawData);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Invalid Base64 for signature {kid}", TrustListItemDSC.kid);
            }
            try
            {
                ContentInfo content = new ContentInfo(dscTrustListRawData);
                SignedCms signedCms = new SignedCms(content, true);

                //Add Signature to CMS message
                signedCms.Decode(dscBytesSignature);

                //Check signature validity 
                signedCms.CheckSignature(true);

                var signedCmsCertificate = signedCms.Certificates[0];
                //Verify against Trust Anchor 
                var isCertificateValid = signedCmsCertificate.Equals(UploadCertificate);

                return isCertificateValid;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to verify DSC certificate with TrustAnchor");
                return false;
            }
        }
    }
}
