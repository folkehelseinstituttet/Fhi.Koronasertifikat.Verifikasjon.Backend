using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Repositories.Enums;
using FHICORC.Domain.Models;
using FHICORC.Integrations.UkGateway.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace FHICORC.Integrations.UkGateway.Services
{
    public class UkGatewayService : IUkGatewayService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ServiceEndpoints _serviceEndpoints;
        private readonly ILogger<UkGatewayService> _logger;

        public UkGatewayService(IHttpClientFactory httpClientFactory, ServiceEndpoints serviceEndpoints, ILogger<UkGatewayService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _serviceEndpoints = serviceEndpoints;
            _logger = logger;
        }

        public Task<List<EuDocSignerCertificate>> GetTrustListAsync(SpecialCountryCodes countryCode)
        {
            string endpoint;
            switch (countryCode)
            {
                case SpecialCountryCodes.UK:
                    endpoint = _serviceEndpoints.UKTrustListEndpoint;
                    break;
                case SpecialCountryCodes.UK_NI:
                    endpoint = _serviceEndpoints.NITrustListEndpoint;
                    break;
                case SpecialCountryCodes.UK_SC:
                    endpoint = _serviceEndpoints.SCTrustListEndpoint;
                    break;
                default:
                    throw new ArgumentException("countryCode must be either 'UK', 'UK_NI' or 'UK_SC'", nameof(countryCode));
            }

            return GetTrustListInternalAsync(countryCode, endpoint);
        }

        private async Task<List<EuDocSignerCertificate>> GetTrustListInternalAsync(SpecialCountryCodes countryCode, string endpoint)
        {
            var httpClient = _httpClientFactory.CreateClient();
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, endpoint);
            
            var response = await httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();
            var responseJsonString = await response.Content.ReadAsStringAsync();
            var data = JArray.Parse(responseJsonString);

            List<EuDocSignerCertificate> ukCertificates = new List<EuDocSignerCertificate>();
            foreach (var kidPkPair in data.Cast<dynamic>())
            {
                ukCertificates.Add(new EuDocSignerCertificate
                {
                    CertificateType = "DSC",
                    Country = countryCode.ToString(),
                    KeyIdentifier = (string)kidPkPair.kid,
                    PublicKey = (string)kidPkPair.publicKey
                });
            }

            _logger.LogInformation("Retrieved {count} ", ukCertificates.Count);

            return ukCertificates;
        }
    }
}
