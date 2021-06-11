using Microsoft.Extensions.Logging;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Integrations.DGCGateway.Models;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using FHICORC.Integrations.DGCGateway.Util.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace FHICORC.Integrations.DGCGateway.Services
{
    public class DgcgResponseVerification :  IDgcgResponseVerification
    {
        private readonly ILogger<DgcgResponseVerification> _logger;
        private readonly CertificateOptions _certificateOptions;
        private readonly ICertificateVerification _certificateVerification;

        public DgcgResponseVerification (ILogger<DgcgResponseVerification> logger, CertificateOptions certificateOptions, ICertificateVerification certificateVerification)
        {
            _logger = logger;
            _certificateOptions = certificateOptions;
            _certificateVerification = certificateVerification; 
        }

        public DgcgTrustListResponseDto VerifyResponseFromGateway(DgcgTrustListResponseDto gatewayResponse)
        {
            var trustAnchor = new X509Certificate2(_certificateOptions.DGCGTrustAnchorPath);

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
                    continue;
                }

                List<X509Certificate2> uploadCertificates = new List<X509Certificate2>();
                foreach (var uploadTrustListItem in uploadTrustListItems)
                {
                    VerifyAgainstTrustAnchor(uploadTrustListItem, trustAnchor, verifiedResponse.TrustListItems, uploadCertificates);
                }

                /* CSCA Verification */
                var cscaTrustListItems = gatewayResponse.TrustListItems.FindAll(x => x.country == country && x.certificateType == CertificateType.CSCA.ToString());
                if (cscaTrustListItems.Count == 0)
                {
                    _logger.LogError("Failed to find CSCA Certificate(s) from {country} in trustlist.", country);
                    continue;
                }

                List<X509Certificate2> cscaCertificates = new List<X509Certificate2>();
                foreach (var cscaTrustListItem in cscaTrustListItems)
                {
                    VerifyAgainstTrustAnchor(cscaTrustListItem, trustAnchor, verifiedResponse.TrustListItems, cscaCertificates);
                }

                /* DSC Verification */
                verifiedResponse.TrustListItems.AddRange(VerifyAndGetDscs(gatewayResponse, uploadCertificates, cscaCertificates, country));
            }
            return verifiedResponse;

        }

        public bool VerifyAgainstTrustAnchor(DgcgTrustListItem trustListItem, X509Certificate2 trustAnchor,
            List<DgcgTrustListItem> verifiedTrustListItems, List<X509Certificate2> verifiedCertificates)
        {
            var validSignature = _certificateVerification.VerifyItemByAnchorSignature(trustListItem, trustAnchor, "TrustAnchor");
            if (!validSignature)
            {
                _logger.LogError("Failed to verify certificate type {certificateType} for {country} with TrustAnchor",
                    trustListItem.certificateType, trustListItem.country);
                return false;
            }

            var cert = new X509Certificate2(Convert.FromBase64String(trustListItem.rawData));
            if (cert.NotAfter < DateTime.Now)
            {
                _logger.LogInformation("{certificateType} certificate {thumbprint} for {country} expired at {expiryDate}", 
                    trustListItem.certificateType, cert.Thumbprint, trustListItem.country, cert.NotAfter);
                return false;
            }

            verifiedTrustListItems.Add(trustListItem);
            verifiedCertificates.Add(cert);

            return true;
        }

        public List<DgcgTrustListItem> VerifyAndGetDscs(DgcgTrustListResponseDto gatewayResponse,
            List<X509Certificate2> uploadCertList, List<X509Certificate2> cscaCertList, string country)
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
                if (uploadCertList.All(up => !_certificateVerification.VerifyItemByAnchorSignature(dscItem, up, "Upload")))
                {
                    _logger.LogInformation("Failed to verify DSC is signed by upload certificate, {thumbprint}, {kid}, {country}",
                        dscItem.thumbprint, dscItem.kid, country);
                    continue;
                }
                if (cscaCertList.All(csca => !_certificateVerification.VerifyDscSignedByCsca(dscItem, csca)))
                {
                    _logger.LogInformation("Failed to verify DSC is trusted by CSCA, {thumbprint}, {kid}, {country}",
                        dscItem.thumbprint, dscItem.kid, country);
                    continue;
                }
                verifiedDscs.Add(dscItem);
            }

            return verifiedDscs;
        }
    }
}
