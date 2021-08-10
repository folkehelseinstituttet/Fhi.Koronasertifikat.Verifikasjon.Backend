using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FHICORC.Application.Models.Options;
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

        public async Task<List<EuDocSignerCertificate>> GetTrustListAsync()
        {
            var httpClient = _httpClientFactory.CreateClient();
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, _serviceEndpoints.UKTrustListEndpoint);
            
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
                    Country = "UK",
                    KeyIdentifier = (string)kidPkPair.kid,
                    PublicKey = (string)kidPkPair.publicKey
                });
            }

            _logger.LogInformation("Retrieved {count} ", ukCertificates.Count);

            return ukCertificates;
        }
    }
}
