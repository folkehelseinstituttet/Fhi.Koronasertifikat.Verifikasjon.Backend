using System;
using System.Linq;
using System.Threading.Tasks;
using FHICORC.Application.Models;
using Newtonsoft.Json;
using FHICORC.Application.Models.Options;
using System.Security.Cryptography.X509Certificates;
using RestSharp;
using System.Net;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using FHICORC.Integrations.DGCGateway.Util;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;
using Org.BouncyCastle.X509;
using System.IO;
using System.Security.Cryptography.Pkcs;
using Org.BouncyCastle.Cms;
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
            await AddTls(client);
            
            var restRequest = new RestRequest(certificateType, Method.GET);

            if (_certificateOptions.DisableDGCGServerCertValidation)
            {
                _logger.LogWarning("DGCG server certificate validation is disabled.");
                client.RemoteCertificateValidationCallback += (sender, cert, chain, error) => { return true; };
            }

            var response = await client.ExecuteGetAsync(restRequest);
            
            DgcgTrustListItem[] parsedResponse;
            try
            {
                parsedResponse = JsonConvert.DeserializeObject<DgcgTrustListItem[]>(response.Content);
            }
            catch (Exception e)
            {
                _logger.LogError("Response parse failed (status code {StatusCode}): {Content} - {Exception}", response.StatusCode, response.Content, e);
                throw;
            }
            
            _logger.LogDebug("DGCG Response {StatusCode} {Content}", response.StatusCode, response.Content);
            _logger.LogDebug("DGCG ParsedResponse {ItemCount}", parsedResponse == null ? "empty" : parsedResponse.Length);
            var resultDto = new DgcgTrustListResponseDto();
            if (parsedResponse == null)
            {
                _logger.LogError("Could not JSON deserialize response from DGCG. {StatusCode} - {ErrorMessage} - {Exception} - {Content}", response.StatusCode, response.ErrorMessage, response.ErrorException, response.Content);
                throw new GeneralDgcgFaultException("Parsed response is null"); 
            }
            else
            {
                resultDto.TrustListItems = parsedResponse.ToList();
            }
            return resultDto;
        }


        public async Task<DgcgRevocationBatchListRespondDto> FetchRevocationBatchListAsync(DateTime date)
        {
            var client = new RestClient(_serviceEndpoints.DGCGRevocationEndpoint);
            await AddTls(client);

            var parsedResponse = new DgcgRevocationBatchListRespondDto(false, new List<DgcgRevocationListBatchItem>());
            var more = false;
            

            do
            {
                var restRequest = new RestRequest(Method.GET);
                if (_certificateOptions.DisableDGCGServerCertValidation)
                {
                    _logger.LogWarning("DGCG server certificate validation is disabled.");
                    client.RemoteCertificateValidationCallback += (sender, cert, chain, error) => { return true; };
                }

                client.ConfigureWebRequest(request =>
                {
                    var methodInfo = request.Headers.GetType().GetMethod("AddWithoutValidate",
                      BindingFlags.Instance | BindingFlags.NonPublic);
                    methodInfo?.Invoke(request.Headers, new[] { "If-Modified-Since", date.ToString("yyyy-MM-ddTHH:mm:ssZ") });
                });


                var response = await client.ExecuteGetAsync(restRequest);

          
                try
                {
                    var _tmpParsedResponse = JsonConvert.DeserializeObject<DgcgRevocationBatchListRespondDto>(response.Content);
                    parsedResponse.Batches.AddRange(_tmpParsedResponse.Batches);
                    more = _tmpParsedResponse.More;
                    date = _tmpParsedResponse.Batches.LastOrDefault().Date;
                }
                catch (NullReferenceException e)
                {
                    _logger.LogInformation("No content in response data", response.StatusCode, response.Content, e);
                    return parsedResponse;
                }
                catch (Exception e)
                {
                    _logger.LogError(" failed (status code {StatusCode}): {Content} - {Exception}", response.StatusCode, response.Content, e);
                    throw;
                }

                _logger.LogDebug("DGCG Response {StatusCode} {Content}", response.StatusCode, response.Content);
                _logger.LogDebug("DGCG ParsedResponse {ItemCount}", parsedResponse == null ? "empty" : parsedResponse.Batches.Count);
            } while (more);

            return parsedResponse;
        }


        public async Task<DGCGRevocationBatchRespondDto> FetchRevocationBatchAsync(string batchId)
        {
            var client = new RestClient(_serviceEndpoints.DGCGRevocationEndpoint + "/" + batchId);
            await AddTls(client);


            var restRequest = new RestRequest(Method.GET);
            restRequest.AddHeader("Accept", "application/cms-text;charset=UTF-8");
            if (_certificateOptions.DisableDGCGServerCertValidation)
            {
                _logger.LogWarning("DGCG server certificate validation is disabled.");
                client.RemoteCertificateValidationCallback += (sender, cert, chain, error) => { return true; };
            }

            var response = await client.ExecuteGetAsync(restRequest);

            DGCGRevocationBatchRespondDto parsedResponse;
            try
            {
                var encodedMessage = Convert.FromBase64String(response.Content);

                var signedCms = new SignedCms();
                signedCms.Decode(encodedMessage);
                signedCms.CheckSignature(true);

                var decodedMessage = Encoding.UTF8.GetString(signedCms.ContentInfo.Content);
                parsedResponse = JsonConvert.DeserializeObject<DGCGRevocationBatchRespondDto>(decodedMessage);

            }
            catch (Exception e)
            {
                _logger.LogError("Response parse failed (status code {StatusCode}): {Content} - {Exception}", response.StatusCode, response.Content, e);
                throw;
            }

            _logger.LogDebug("DGCG Response {StatusCode} {Content}", response.StatusCode, response.Content);
            _logger.LogDebug("DGCG ParsedResponse {ItemCount}", parsedResponse == null ? "empty" : parsedResponse.Entries.Count);

            return parsedResponse;

        }

        private async Task<X509Certificate2> FetchTlsCertificate()
        {
            if (!string.IsNullOrEmpty(_certificateOptions.NBTlsKeyVaultUrl) &&
                !string.IsNullOrEmpty(_certificateOptions.NBTlsKeyVaultCertificateName))
            {
                var keyVaultUri = new Uri(_certificateOptions.NBTlsKeyVaultUrl);
                var certificateName = _certificateOptions.NBTlsKeyVaultCertificateName;
                var credential = new DefaultAzureCredential();

                var certClient = new CertificateClient(keyVaultUri, credential);
                var secretClient = new SecretClient(keyVaultUri, credential);
                var certificate = (await certClient.GetCertificateAsync(certificateName)).Value;

                Uri secretId = certificate.SecretId;
                var segments = secretId.Segments;
                string secretName = segments[2].Trim('/');
                string version = segments[3].TrimEnd('/');

                var secret = (await secretClient.GetSecretAsync(secretName, version)).Value;
                return new X509Certificate2(Base64Util.FromString(secret.Value));
            }

            return new X509Certificate2(_certificateOptions.NBTlsCertificatePath, _certificateOptions.NBTlsCertificatePassword);
        }



        private async Task AddTls(RestClient client) {

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.DefaultConnectionLimit = 9999;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            //Fetch TLS certificate 
            var certificate = await FetchTlsCertificate();
            client.ClientCertificates = new X509CertificateCollection() { certificate };
            client.Proxy = new WebProxy(); // new WebProxy("127.0.0.1", 8888);

        }
    }
}
