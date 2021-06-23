using System;
using System.Collections.Generic;
using System.Linq;
using FHICORC.Application.Models;
using FHICORC.Integrations.DGCGateway.Util.Interfaces;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace FHICORC.Integrations.DGCGateway.Util
{
    public class BcCertificateVerification : ICertificateVerification<X509Certificate>
    {
        private readonly ILogger<BcCertificateVerification> _logger;

        public BcCertificateVerification(ILogger<BcCertificateVerification> logger)
        {
            _logger = logger; 
        }

        public bool VerifyDscSignedByCsca(DgcgTrustListItem trustListItem, List<X509Certificate> cscaCertificates)
        {
            string issuer;

            try
            {
                var parser = new X509CertificateParser();
                var dsc = parser.ReadCertificate(Convert.FromBase64String(trustListItem.rawData));
                issuer = dsc.IssuerDN.ToString();

                foreach (var cscaBc in cscaCertificates)
                {
                    if (dsc.IssuerDN.Equivalent(cscaBc.SubjectDN))
                    {
                        try
                        {
                            dsc.Verify(cscaBc.GetPublicKey());
                        }
                        catch (Exception e)
                        {
                            _logger.LogWarning(e, "CSCA signature of DSC failed for {country}, {kid}",
                                trustListItem.country, trustListItem.kid);
                            return false;
                        }

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Unexpected error verifying DSC certificate chain for {country}, {kid}",
                    trustListItem.country, trustListItem.kid);
                return false;
            }

            _logger.LogWarning("CSCA {issuer} not found for {country}, {kid}",
                issuer, trustListItem.country, trustListItem.kid);
            return false;
        }

        public bool VerifyItemByAnchorSignature(DgcgTrustListItem trustListItem, X509Certificate anchorCertificate,
            string anchorCertificateType)
        {
            return VerifyItemByAnchorSignature(trustListItem, new List<X509Certificate> {anchorCertificate},
                anchorCertificateType);
        }

        public bool VerifyItemByAnchorSignature(DgcgTrustListItem trustListItem, List<X509Certificate> anchorCertificates,
            string anchorCertificateType)
        {
            try
            {
                byte[] signatureData;
                try
                {
                    signatureData = Convert.FromBase64String(trustListItem.signature);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Invalid Base64 string for {signature} for {country}, {kid}, {anchorType}",
                        trustListItem.signature, trustListItem.country, trustListItem.kid, anchorCertificateType);
                    return false;
                }

                byte[] rawData;
                try
                {
                    rawData = Convert.FromBase64String(trustListItem.rawData);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Invalid Base64 string for {rawData} for {country}, {kid}, {anchorType}",
                        trustListItem.rawData, trustListItem.country, trustListItem.kid, anchorCertificateType);
                    return false;
                }

                CmsSignedData cmsSignedData;
                try
                {
                    cmsSignedData = new CmsSignedData(new CmsProcessableByteArray(rawData), signatureData);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Could not parse signature for {country}, {kid}, {anchorType}",
                        trustListItem.country, trustListItem.kid, anchorCertificateType);
                    return false;
                }

                return VerifySignedData(cmsSignedData, trustListItem, anchorCertificates, anchorCertificateType);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Unexpected error validating anchor signature for {country}, {kid}, {anchorType}",
                    trustListItem.country, trustListItem.kid, anchorCertificateType);
                return false;
            }
        }

        private bool VerifySignedData(CmsSignedData cmsSignedData, DgcgTrustListItem trustListItem, List<X509Certificate> anchorCertificates, string anchorType)
        {
            if (!cmsSignedData.SignedContentType.Equals(CmsObjectIdentifiers.Data))
            {
                return Fail("Wrong content type of signed data {SignedContentType} for {country}, {kid}, {anchorType}",
                    cmsSignedData.SignedContentType, trustListItem.country, trustListItem.kid, anchorType);
            }

            var certificateHolderCollection =
                cmsSignedData.GetCertificates("Collection").GetMatches(new X509CertStoreSelector());
            if (certificateHolderCollection.Count != 1)
            {
                return Fail("Certificate holder collection did not have {count} = 1 for {country}, {kid}, {anchorType}",
                    certificateHolderCollection.Count, trustListItem.country, trustListItem.kid, anchorType);
            }

            var certificateHolder = certificateHolderCollection.Cast<X509Certificate>().First();

            var signerInfos = cmsSignedData.GetSignerInfos();
            if (signerInfos.Count != 1)
            {
                return Fail("Signer info {count} was not 1 {country}, {kid}, {anchorType}",
                    signerInfos.Count, trustListItem.country, trustListItem.kid, anchorType);
            }

            var signerInformation = signerInfos.GetSigners().Cast<SignerInformation>().First();

            bool signatureVerified;
            try
            {
                signatureVerified = signerInformation.Verify(certificateHolder);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to run Verify for {country}, {kid}, {anchorType}",
                    trustListItem.country, trustListItem.kid, anchorType);
                return false;
            }

            if (!signatureVerified)
            {
                return Fail("Signature could not be verified for {country}, {kid}, {anchorType}",
                    trustListItem.country, trustListItem.kid, anchorType);
            }

            var anchorUsed = anchorCertificates.Any(anchor => anchor.Equals(certificateHolder));
            if (!anchorUsed)
            {
                return Fail("Correct {anchor} was not used to create the signature {certificateHolder} for {country}, {kid}, {anchorType}",
                    anchorCertificates.Select(s => s.SubjectDN.ToString()).ToArray(), certificateHolder.SubjectDN.ToString(),
                    trustListItem.country, trustListItem.kid, anchorType);
            }

            return true;
        }

        private bool Fail(string message, params object[] parameters)
        {
            _logger.LogWarning(message, parameters);
            return false;
        }
    }
}
