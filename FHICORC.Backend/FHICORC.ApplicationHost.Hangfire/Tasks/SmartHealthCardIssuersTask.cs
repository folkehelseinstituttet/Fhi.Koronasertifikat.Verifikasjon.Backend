using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Models.SmartHealthCard;
using FHICORC.Application.Services.Interfaces;
using FHICORC.ApplicationHost.Hangfire.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace FHICORC.ApplicationHost.Hangfire.Tasks
{
    public class SmartHealthCardIssuersTask : ISmartHealthCardIssuersTask
    {
        // This cannot be moved to AppSettings as it is used in attribute and therefore must be constant.
        public const int DisableConcurrentTimeout = 10;

        private const string JobId = "update-smart-health-card-issuers";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SmartHealthCardIssuersTask> _logger;
        private readonly ITrustedIssuerService _trustedIssuerService;
        private readonly CronOptions _cronOptions;
        private readonly ServiceEndpoints _serviceEndpoints;

        public SmartHealthCardIssuersTask(
            IHttpClientFactory httpClientFactory,
            ILogger<SmartHealthCardIssuersTask> logger,
            ITrustedIssuerService trustedIssuerService,
            CronOptions cronOptions,
            ServiceEndpoints serviceEndpoints)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _trustedIssuerService = trustedIssuerService;
            _cronOptions = cronOptions;
            _serviceEndpoints = serviceEndpoints;
        }

        public void SetupTask()
        {
            _logger.LogInformation($"Adding job {JobId} {_cronOptions.SmartHealthCardIssuersCron}");
            RecurringJob.AddOrUpdate(JobId, () => UpdateSmartHealthCardIssuers(), _cronOptions.SmartHealthCardIssuersCron);
            _logger.LogInformation($"Scheduling {JobId} on startup after {_cronOptions.SmartHealthCardIssuersOnStartupAfterSeconds} seconds");
            BackgroundJob.Schedule(() => UpdateSmartHealthCardIssuers(),
                TimeSpan.FromSeconds(_cronOptions.CountriesReportRepositoryOnStartupAfterSeconds));
        }

        [AutomaticRetry(
            Attempts = 3,
            LogEvents = true,
            OnAttemptsExceeded = AttemptsExceededAction.Fail,
            DelaysInSeconds = new int[] { 60, 120, 180 })]
        [DisableConcurrentExecution(DisableConcurrentTimeout)]
        public async Task UpdateSmartHealthCardIssuers()
        {
            try
            {
                _logger.LogInformation($"Running job {JobId}");

                ShcIssuersDto issuers = await GetIssuers();
                await _trustedIssuerService.ReplaceAutomaticallyAddedIssuers(issuers);

                _logger.LogInformation($"Job {JobId } success");
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Job {JobId} failed. Issuers json http request failed with code {e.StatusCode}");
                throw;
            }
            catch (Exception)
            {
                _logger.LogError($"Job {JobId} failed.");
                throw;
            }
        }

        private async Task<ShcIssuersDto> GetIssuers()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, _serviceEndpoints.SHCIssuerListEndpoint);

            HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();
            string responseJsonString = await response.Content.ReadAsStringAsync();

            ShcIssuersDto issuers = JsonSerializer.Deserialize<ShcIssuersDto>(responseJsonString);
            if (issuers.ParticipatingIssuers == null || issuers.ParticipatingIssuers.Length == 0)
            {
                throw new Exception("Fetched issuer list was empty");
            }
            _logger.LogInformation($"Job {JobId} fetched issuers: {issuers.ParticipatingIssuers.Length}");

            // Clean duplicates, as source list has had that before.
            issuers.ParticipatingIssuers = issuers.ParticipatingIssuers
                .GroupBy(p => p.Iss)
                .Select(g => g.FirstOrDefault())
                .ToArray();

            return issuers;
        }
    }
}
