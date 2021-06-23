using Microsoft.Extensions.Logging;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Integrations.DGCGateway.Models;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using FHICORC.Integrations.DGCGateway.Util.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.X509;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace FHICORC.Integrations.DGCGateway.Services
{
    public class DgcgResponseVerification<TCert> :  IDgcgResponseVerification
    {
        private readonly ILogger<DgcgResponseVerification<TCert>> _logger;
        private readonly CertificateOptions _certificateOptions;
        private readonly ICertificateVerification<TCert> _certificateVerification;

        public DgcgResponseVerification (ILogger<DgcgResponseVerification<TCert>> logger, CertificateOptions certificateOptions,
            ICertificateVerification<TCert> certificateVerification)
        {
            _logger = logger;
            _certificateOptions = certificateOptions;
            _certificateVerification = certificateVerification; 
        }

        public DgcgTrustListResponseDto VerifyResponseFromGateway(DgcgTrustListResponseDto gatewayResponse)
        {
            var trustAnchor = LoadCertificate(File.ReadAllBytes(_certificateOptions.DGCGTrustAnchorPath));

            var countries = gatewayResponse.TrustListItems.Select(x => x.country).Distinct().ToList();
            var verifiedResponse = new DgcgTrustListResponseDto();
            verifiedResponse.TrustListItems = new List<DgcgTrustListItem>(); 
            
            foreach (var country in countries)
            {
                /* Upload Verification */
                var uploadTrustListItems = gatewayResponse.TrustListItems.FindAll(x => x.country == country && x.certificateType == CertificateType.UPLOAD.ToString());
                if (uploadTrustListItems.Count == 0)
                {
                    _logger.LogError("Failed to find upload certificate from {country} in trustlist.", country);
                }

                (List<DgcgTrustListItem> verifiedUpItems, List<TCert> uploadCertificates) = VerifyAgainstTrustAnchor(uploadTrustListItems, trustAnchor);
                verifiedResponse.TrustListItems.AddRange(verifiedUpItems);

                /* CSCA Verification */
                var cscaTrustListItems = gatewayResponse.TrustListItems.FindAll(x => x.country == country && x.certificateType == CertificateType.CSCA.ToString());
                if (cscaTrustListItems.Count == 0)
                {
                    _logger.LogError("Failed to find CSCA Certificate(s) from {country} in trustlist.", country);
                }

                (List<DgcgTrustListItem> verifiedCscaItems, List<TCert> cscaCertificates) = VerifyAgainstTrustAnchor(cscaTrustListItems, trustAnchor);
                verifiedResponse.TrustListItems.AddRange(verifiedCscaItems);

                /* DSC Verification */
                if (uploadTrustListItems.Count == 0 || cscaTrustListItems.Count == 0)
                {
                    continue;
                }

                verifiedResponse.TrustListItems.AddRange(VerifyAndGetDscs(gatewayResponse, uploadCertificates, cscaCertificates, country));
            }

            return verifiedResponse;
        }

        public (List<DgcgTrustListItem> TrustListItems, List<TCert> Certificates) VerifyAgainstTrustAnchor(
            List<DgcgTrustListItem> trustListItems, TCert trustAnchor)
        {
            var verifiedItems = new List<DgcgTrustListItem>();
            var verifiedCertificates = new List<TCert>();
            foreach (var trustListItem in trustListItems)
            {
                var validSignature = _certificateVerification.VerifyItemByAnchorSignature(trustListItem, trustAnchor, "TrustAnchor");
                if (!validSignature)
                {
                    _logger.LogError("Failed to verify certificate type {certificateType} for {country} with TrustAnchor",
                        trustListItem.certificateType, trustListItem.country);
                    continue;
                }

                TCert cert;
                try
                {
                    cert = LoadCertificate(Convert.FromBase64String(trustListItem.rawData));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to initialize certificate of {certificateType} for country {country}",
                        trustListItem.certificateType, trustListItem.country);
                    continue;
                }

                var expiry = GetExpiry(cert);
                if (expiry < DateTime.Now)
                {
                    _logger.LogInformation("{certificateType} certificate {thumbprint} for {country} expired at {expiryDate}", 
                        trustListItem.certificateType, GetThumbprint(cert), trustListItem.country, expiry);
                    continue;
                }

                verifiedItems.Add(trustListItem);
                verifiedCertificates.Add(cert);
            }

            return (verifiedItems, verifiedCertificates);
        }

        public List<DgcgTrustListItem> VerifyAndGetDscs(DgcgTrustListResponseDto gatewayResponse,
            List<TCert> uploadCertList, List<TCert> cscaCertList, string country)
        {
            var dscItemList = gatewayResponse.TrustListItems.FindAll(x => x.country == country && x.certificateType == CertificateType.DSC.ToString());
            var verifiedDscs = new List<DgcgTrustListItem>(); 

            if (dscItemList.Count == 0)
            {
                _logger.LogError("Failed to find DSC certificate for {country}", country);
                return new List<DgcgTrustListItem>();
            }

            foreach (var dscItem in dscItemList)
            {
                if (!_certificateVerification.VerifyItemByAnchorSignature(dscItem, uploadCertList, "Upload"))
                {
                    _logger.LogInformation("Failed to verify DSC is signed by upload certificate, {thumbprint}, {kid}, {country}",
                        dscItem.thumbprint, dscItem.kid, country);
                    continue;
                }
                if (!_certificateVerification.VerifyDscSignedByCsca(dscItem, cscaCertList))
                {
                    _logger.LogInformation("Failed to verify DSC, {thumbprint}, {kid}, {country}",
                        dscItem.thumbprint, dscItem.kid, country);
                    continue;
                }
                verifiedDscs.Add(dscItem);
            }

            return verifiedDscs;
        }

        private static TCert LoadCertificate(byte[] rawData)
        {
            if (typeof(TCert) == typeof(X509Certificate2))
            {
                return (TCert)(object)new X509Certificate2(rawData);
            }

            if (typeof(TCert) == typeof(X509Certificate))
            {
                var parser = new X509CertificateParser();
                return (TCert)(object)parser.ReadCertificate(rawData);
            }

            throw new InvalidOperationException("Only System.Security.Cryptography.X509Certificates.X509Certificate2 and Org.BouncyCastle.X509.X509Certificate is supported");
        }

        private static string GetThumbprint(TCert cert)
        {
            switch (cert)
            {
                case X509Certificate2 sscCert:
                    return sscCert.Thumbprint;
                case X509Certificate bcCert:
                    var encoded = bcCert.GetEncoded();
                    var fingerprint = new StringBuilder();
                    var sha1 = new Sha1Digest();
                    var data = new byte[20];

                    sha1.BlockUpdate(encoded, 0, encoded.Length);
                    sha1.DoFinal(data, 0);

                    for (int i = 0; i < data.Length; i++)
                    {
                        fingerprint.Append(data[i].ToString("X2"));
                    }

                    return fingerprint.ToString();
                default:
                    throw new InvalidOperationException("Only System.Security.Cryptography.X509Certificates.X509Certificate2 and Org.BouncyCastle.X509.X509Certificate is supported");
            }
        }

        private static DateTime GetExpiry(TCert cert)
        {
            switch (cert)
            {
                case X509Certificate2 sscCert:
                    return sscCert.NotAfter;
                case X509Certificate bcCert:
                    return bcCert.NotAfter;
                default:
                    throw new InvalidOperationException("Only System.Security.Cryptography.X509Certificates.X509Certificate2 and Org.BouncyCastle.X509.X509Certificate is supported");
            }
        }
    }
}
