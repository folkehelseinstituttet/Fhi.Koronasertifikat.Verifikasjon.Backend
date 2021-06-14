using Microsoft.Extensions.Logging;
using FHICORC.Application.Models;
using FHICORC.Integrations.DGCGateway.Util.Interfaces;
using System;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace FHICORC.Integrations.DGCGateway.Util
{
    public class CertificateVerification: ICertificateVerification
    {
        private readonly ILogger<CertificateVerification> _logger;

        public CertificateVerification (ILogger<CertificateVerification> logger)
        {
            _logger = logger; 
        }

        public bool VerifyDscSignedByCsca(DgcgTrustListItem trustListItem, X509Certificate2 cscaCertificate)
        {
            var dscCert = new X509Certificate2(Convert.FromBase64String(trustListItem.rawData));

            X509Chain chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
            chain.ChainPolicy.VerificationTime = DateTime.Now;
            chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 0, 0);
            chain.ChainPolicy.ExtraStore.Add(cscaCertificate);

            bool isChainValid = chain.Build(dscCert);

            if (!isChainValid)
            {
                string[] errors = chain.ChainStatus
                    .Select(x => $"{x.StatusInformation.Trim()} ({x.Status})")
                    .ToArray();
                string certificateErrorsString = "Unknown errors.";

                if (errors.Length > 0)
                {
                    certificateErrorsString = String.Join(", ", errors);
                }

                _logger.LogWarning("Trust chain did not complete to the known authority anchor. {errors}", certificateErrorsString);
                return false;
            }

            var result = chain.ChainElements
                .Cast<X509ChainElement>()
                .Any(x => x.Certificate.Thumbprint == cscaCertificate.Thumbprint);

            return result;
        }

        public bool VerifyItemByAnchorSignature(DgcgTrustListItem trustListItem, X509Certificate2 anchorCertificate, string anchorCertificateType)
        {
            byte[] cmsBytesSignature;
            byte[] trustListRawData;

            try
            {
                cmsBytesSignature = Convert.FromBase64String(trustListItem.signature);
                trustListRawData = Convert.FromBase64String(trustListItem.rawData);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Invalid Base64 for signature/rawData for {kid}", trustListItem.kid);
                return false;
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
                var isCertificateValid = signedCmsCertificate.Equals(anchorCertificate);

                return isCertificateValid;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to verify {certificateType} certificate with {anchorCertificateType}",
                    trustListItem.certificateType, anchorCertificateType);
                return false;
            }
        }
    }
}
