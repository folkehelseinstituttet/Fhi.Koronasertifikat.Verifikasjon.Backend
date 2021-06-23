using Microsoft.Extensions.Logging;
using FHICORC.Application.Models;
using FHICORC.Domain.Models;
using FHICORC.Integrations.DGCGateway.Models;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using FHICORC.Integrations.DGCGateway.Util;

namespace FHICORC.Integrations.DGCGateway.Services
{
    public class DgcgResponseParser: IDgcgResponseParser
    {
        private readonly ILogger<DgcgResponseParser> _logger;
        

        public DgcgResponseParser(ILogger<DgcgResponseParser> logger)
        {
            _logger = logger;
        }
        public List<EuDocSignerCertificate> ParseToEuDocSignerCertificate(DgcgTrustListResponseDto dgcgTrustListResponseDto)
        {
            var docSignerCerts = dgcgTrustListResponseDto.TrustListItems.Where(item => item.certificateType == CertificateType.DSC.ToString());

            _logger.LogDebug("Parsing DSC items from DGCG Gateway, {itemcount}", docSignerCerts.Count()); 
            var EuDocsignerCertificateList = new List<EuDocSignerCertificate>();
            int extractCount = 0;
            foreach (var trustListItem in docSignerCerts)
            {
                var DSC = new EuDocSignerCertificate()
                {
                    KeyIdentifier = trustListItem.kid,
                    Country = trustListItem.country,
                    CertificateType = trustListItem.certificateType,
                    Thumbprint = trustListItem.thumbprint,
                    Signature = trustListItem.signature,
                    Certificate = trustListItem.rawData,
                    Timestamp = trustListItem.timestamp,
                };

                DSC.PublicKey = ExtractPublicKey(trustListItem.rawData);
                if (!string.IsNullOrEmpty(DSC.PublicKey))
                {
                    extractCount++;
                }
                EuDocsignerCertificateList.Add(DSC);
            }
            _logger.LogDebug("Successfully parsed {itemcount} DSC items, {extractCount} public keys extracted.", EuDocsignerCertificateList.Count, extractCount);

            return EuDocsignerCertificateList;
        }

        private string ExtractPublicKey(string rawData)
        {

            var cert = new X509Certificate2(Base64Util.FromString(rawData));
            byte[] publicKeyAsBytes;
            try
            {
                if (cert.GetECDsaPublicKey() != null)
                {
                    publicKeyAsBytes = cert.GetECDsaPublicKey().ExportSubjectPublicKeyInfo();
                }
                else
                {
                    publicKeyAsBytes = cert.PublicKey.Key.ExportSubjectPublicKeyInfo();
                }
                var publicKeyEncoded = Convert.ToBase64String(publicKeyAsBytes);
                return publicKeyEncoded;
            }                                                 
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to extract public key from certificate {thumbprint}", cert.Thumbprint);
            }
            return "";
        }
    }
}
