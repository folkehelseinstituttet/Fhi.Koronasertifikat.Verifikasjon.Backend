
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
                /* CSCA Verification */
                var CSCATrustListItems = gatewayResponse.TrustListItems.FindAll(x => x.country == country && x.certificateType == CertificateType.CSCA.ToString());
                var CSCATrustListItem = CSCATrustListItems.OrderByDescending(x => x.timestamp).FirstOrDefault();


                if (CSCATrustListItem == null)
                {
                    _logger.LogError("Failed to find latest CSCA Certificate from {country} in trustlist.", country);
                    continue;
                }
                if (!VerifyCSCAAgainstTrustAnchor(CSCATrustListItem, trustAnchor)) continue;

                verifiedResponse.TrustListItems.Add(CSCATrustListItem);

                /* Upload Verification */
                var uploadCerts = gatewayResponse.TrustListItems.FindAll(x => x.country == CSCATrustListItem.country && x.certificateType == CertificateType.UPLOAD.ToString());
                var UploadTrustListitem = uploadCerts.OrderByDescending(x => x.timestamp).FirstOrDefault();

                if (!VerifyUploadAgainstTrustAnchor(UploadTrustListitem, CSCATrustListItem, trustAnchor)) continue;

                verifiedResponse.TrustListItems.Add(UploadTrustListitem);

                /* DSC Verification */
                verifiedResponse.TrustListItems.AddRange(VerifyAndGetDSCs(gatewayResponse, CSCATrustListItem, UploadTrustListitem));
            }
            return verifiedResponse;

        }

        public bool VerifyCSCAAgainstTrustAnchor(DgcgTrustListItem CSCATrustListItem, X509Certificate2 trustAnchor)
        {
            return _certificateVerification.VerifyByTrustAnchorSignature(CSCATrustListItem, trustAnchor);
        }

        public bool VerifyUploadAgainstTrustAnchor(DgcgTrustListItem uploadTrustListItem, DgcgTrustListItem CSCATrustListItem, X509Certificate2 trustAnchor)
        {
            if (uploadTrustListItem == null)
            {
                _logger.LogError("Failed to find upload certificate for {country}", CSCATrustListItem.country);
                return false;
            }
            return _certificateVerification.VerifyByTrustAnchorSignature(uploadTrustListItem, trustAnchor);
        }

        public List<DgcgTrustListItem> VerifyAndGetDSCs(DgcgTrustListResponseDto gatewayResponse, DgcgTrustListItem CSCATrustListItem, DgcgTrustListItem TrustListItemUpload)
        {
            var CSCACert = new X509Certificate2(Convert.FromBase64String(CSCATrustListItem.rawData));
            var DSCItemList = gatewayResponse.TrustListItems.FindAll(x => x.country == CSCATrustListItem.country && x.certificateType == CertificateType.DSC.ToString());
            var uploadCert = new X509Certificate2(Convert.FromBase64String(TrustListItemUpload.rawData));
            var verifiedDSCs = new List<DgcgTrustListItem>(); 

            if (DSCItemList.Count == 0)
            {
                _logger.LogError("Failed to find DSC certificate for {country}", CSCATrustListItem.country);
                return new List<DgcgTrustListItem>();
            }
            foreach (var DSCItem in DSCItemList)
            {
                if (!_certificateVerification.VerifyDSCSignedByCSCA(DSCItem, CSCACert))
                {
                    _logger.LogInformation("Failed to verify DSC is trusted by CSCA, {thumbprint}", DSCItem.thumbprint);
                    continue;
                }
                if (!_certificateVerification.VerifyDSCByUploadCertificate(DSCItem, uploadCert))
                {
                    _logger.LogInformation("Failed to verify DSC is signed by TrustAnchor, {thumbprint}", DSCItem.thumbprint);
                    continue;
                }
                verifiedDSCs.Add(DSCItem);
            }
            return verifiedDSCs;
        }
    }
}
