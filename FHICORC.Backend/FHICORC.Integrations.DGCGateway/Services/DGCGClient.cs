
using System.Linq;
using System.Threading.Tasks;
using FHICORC.Application.Models;
using Newtonsoft.Json;
using FHICORC.Application.Models.Options;
using System.Security.Cryptography.X509Certificates;
using RestSharp;
using System.Net;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace FHICORC.Integrations.DGCGateway.Services
{
    public class DgcgClient : IDgcgClient
    {
        private readonly ServiceEndpoints _serviceEndpoints;
        private readonly CertificateOptions _certificateOptions;
        private readonly ILogger<DgcgClient> _logger;

        public DgcgClient(ILogger<DgcgClient> logger, ServiceEndpoints serviceEndpoints, CertificateOptions certificateOptions)
        {
            _serviceEndpoints = serviceEndpoints;
            _certificateOptions = certificateOptions;
            _logger = logger;
        }
        public async Task<DgcgTrustListResponseDto> FetchTrustListAsync(string certificateType)
        {
            var client = new RestClient(_serviceEndpoints.DGCGTrustListEndpoint);

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.DefaultConnectionLimit = 9999;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            //Fetch TLS certificate 
            X509Certificate2 certificate = new X509Certificate2(_certificateOptions.NBTlsCertificatePath);

            client.ClientCertificates = new X509CertificateCollection() { certificate };
            client.Proxy = new WebProxy();
            var restRequest = new RestRequest(certificateType, Method.GET);

            var response = await client.ExecuteGetAsync(restRequest);
            var parsedResponse = JsonConvert.DeserializeObject<DgcgTrustListItem[]>(response.Content);

            var resultDto = new DgcgTrustListResponseDto();
            if (parsedResponse == null)
            {
                _logger.LogError("Could not JSON deserialize response from DGCG. {StatusCode} - {ErrorMessage} - {Exception} - {Content}", response.StatusCode, response.ErrorMessage, response.ErrorException, response.Content);
                resultDto.TrustListItems = new List<DgcgTrustListItem>();
            }
            else
            {
                resultDto.TrustListItems = parsedResponse.ToList();
            }
            return resultDto;
        }
    }
}
