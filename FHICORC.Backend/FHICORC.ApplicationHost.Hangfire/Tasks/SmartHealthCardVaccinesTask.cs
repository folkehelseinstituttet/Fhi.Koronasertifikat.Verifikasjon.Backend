using System;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Models.SmartHealthCard.VaccineSystems;
using FHICORC.ApplicationHost.Hangfire.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using FHICORC.Domain.Models;
using FHICORC.Application.Models.SmartHealthCard;
using Microsoft.VisualBasic.FileIO;
using FHICORC.Application.Services.Interfaces;

namespace FHICORC.ApplicationHost.Hangfire.Tasks
{
    public class SmartHealthCardVaccinesTask : ISmartHealthCardVaccinesTask
    {
        // This cannot be moved to AppSettings as it is used in attribute and therefore must be constant.
        public const int DisableConcurrentTimeout = 10;

        private const string JobId = "update-smart-health-card-vaccines";
        private const string VaccineTarget = "Covid-19";
        private const string CvxCovid19Keyword = "COVID-19";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SmartHealthCardVaccinesTask> _logger;
        private readonly IVaccineCodesService _vaccineCodesService;
        private readonly CronOptions _cronOptions;
        private readonly ServiceEndpoints _serviceEndpoints;

        public SmartHealthCardVaccinesTask(
            IHttpClientFactory httpClientFactory,
            ILogger<SmartHealthCardVaccinesTask> logger,
            IVaccineCodesService vaccineCodesService,
            CronOptions cronOptions,
            ServiceEndpoints serviceEndpoints)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _vaccineCodesService = vaccineCodesService;
            _cronOptions = cronOptions;
            _serviceEndpoints = serviceEndpoints;
        }

        public void SetupTask()
        {
            _logger.LogInformation($"Adding job {JobId} {_cronOptions.SmartHealthCardVaccinesCron}");
            RecurringJob.AddOrUpdate(JobId, () => UpdateSmartHealthCardVaccines(), _cronOptions.SmartHealthCardVaccinesCron);
            _logger.LogInformation($"Scheduling {JobId} on startup after {_cronOptions.SmartHealthCardVaccinesOnStartupAfterSeconds} seconds");
            BackgroundJob.Schedule(() => UpdateSmartHealthCardVaccines(),
                TimeSpan.FromSeconds(_cronOptions.SmartHealthCardVaccinesOnStartupAfterSeconds));
        }

        [AutomaticRetry(
            Attempts = 3,
            LogEvents = true,
            OnAttemptsExceeded = AttemptsExceededAction.Fail,
            DelaysInSeconds = new int[] { 60, 120, 180 })]
        [DisableConcurrentExecution(DisableConcurrentTimeout)]
        public async Task UpdateSmartHealthCardVaccines()
        {
            try
            {
                _logger.LogInformation($"Running job {JobId}");

                await UpdateCvxVaccines();
                // Add more vaccine code systems here, or rename file to only cover CVX. (one for each)

                _logger.LogInformation($"Job {JobId } success");
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Job {JobId} failed. Vaccine http request failed with code {e.StatusCode}");
                throw;
            }
            catch (Exception)
            {
                _logger.LogError($"Job {JobId} failed.");
                throw;
            }
        }

        private async Task UpdateCvxVaccines()
        {
            ShcCvxVaccineListDto cvxVaccines = await GetCvxVaccines();
            IEnumerable<VaccineCodesModel> vaccines = cvxVaccines.CvxVaccines
                .Where(x => x.CdcProductName.ToUpper().Contains(CvxCovid19Keyword.ToUpper()))
                .Select(x => new VaccineCodesModel()
                {
                    VaccineCode = x.CvxCode,
                    CodingSystem = CodingSystem.Cvx,
                    Name = x.ShortDescription,
                    Manufacturer = x.Manufacturer,
                    Type = "", // not known
                    Target = VaccineTarget,
                    IsAddManually = false
                });

            _logger.LogInformation($"Covid-19 cvx vaccines fetched: {vaccines.Count()}");

            await _vaccineCodesService.ReplaceAutomaticVaccines(vaccines, CodingSystem.Cvx);
        }

        private async Task<ShcCvxVaccineListDto> GetCvxVaccines()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, _serviceEndpoints.SHCVaccineCvxListEndpoint);

            HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();
            Stream responseCsv = await response.Content.ReadAsStreamAsync();
            using var csvStream = new StreamReader(responseCsv);

            ShcCvxVaccineListDto cvxVaccines = new();
            using (TextFieldParser parser = new(csvStream))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters("|");
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    if (fields.Length > 1) // Dont add if just a single field.
                    {
                        cvxVaccines.CvxVaccines.Add(new(fields));
                    }
                }
            }

            if (cvxVaccines.CvxVaccines == null || cvxVaccines.CvxVaccines.Count == 0)
            {
                throw new Exception("Fetched cvx vaccine list was empty");
            }
            _logger.LogInformation($"Job {JobId} fetched cvx vaccines: {cvxVaccines.CvxVaccines.Count}");

            return cvxVaccines;
        }
    }
}
