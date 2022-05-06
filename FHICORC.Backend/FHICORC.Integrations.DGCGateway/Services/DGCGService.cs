using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using System;
using System.IO;

namespace FHICORC.Integrations.DGCGateway.Services
{
    public class DgcgService : IDgcgService
    {
        private readonly ILogger<DgcgService> _logger;
        private readonly IDgcgResponseVerification _dgcgResponseVerification;
        private readonly FeatureToggles _featureToggles;
        private readonly IDgcgClient _dgcgClient;

        public DgcgService(ILogger<DgcgService> logger, IDgcgClient dgcgClient, IDgcgResponseVerification dgcgResponseVerification, FeatureToggles featureToggles)
        {
            _logger = logger;
            _dgcgResponseVerification = dgcgResponseVerification;
            _featureToggles = featureToggles;
            _dgcgClient = dgcgClient;
        }

        public async Task<DgcgTrustListResponseDto> GetTrustListAsync(string certificateType = "")
        {

            var fullResponse = await _dgcgClient.FetchTrustListAsync(certificateType);

            var certificateCount = fullResponse.TrustListItems.Count;
            if (_featureToggles.DisableTrustListVerification)
            {
                return fullResponse; 
            }
            else
            {
                var verifiedResponse = _dgcgResponseVerification.VerifyResponseFromGateway(fullResponse);
                if (verifiedResponse.TrustListItems.Count < certificateCount)
                {
                    _logger.LogInformation("Unsuccessfully verified DSC(S) {count} ", certificateCount - verifiedResponse.TrustListItems.Count);
                }
                _logger.LogInformation("Verified successfully {count} ", verifiedResponse.TrustListItems.Count);
                return verifiedResponse;
            }
        }

        public async Task<DgcgRevocationBatchListRespondDto> GetRevocationBatchListAsync(string modifiedSince) 
        {

            var d = "2022-05-23T00:00:00Z";
            var fullResponse = await _dgcgClient.FetchRevocationBatchListAsync(d);

            return fullResponse;

        }

        public async Task<DGCGRevocationBatchRespondDto> GetRevocationBatchAsync(string batchId)
        {

            var fullResponse = await _dgcgClient.FetchRevocationBatchAsync(batchId);
            return fullResponse;
        }
    }   
}
