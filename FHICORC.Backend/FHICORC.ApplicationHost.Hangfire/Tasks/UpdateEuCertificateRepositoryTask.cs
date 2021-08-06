using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using FHICORC.Application.Common.Interfaces;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Repositories;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.ApplicationHost.Hangfire.Interfaces;
using FHICORC.Integrations.DGCGateway;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using FHICORC.Integrations.UkGateway.Services.Interfaces;

namespace FHICORC.ApplicationHost.Hangfire.Tasks
{
    public class UpdateEuCertificateRepositoryTask : IUpdateEuCertificateRepositoryTask
    {
        // This cannot be moved to AppSettings as it is used in attribute and therefore must be constant.
        public const int DisableConcurrentTimeout = 10;

        private readonly ILogger<UpdateEuCertificateRepositoryTask> _logger;
        private readonly CronOptions _cronOptions;
        private readonly FeatureToggles _featureToggles;
        private readonly IDgcgService _dgcgService;
        private readonly IDgcgResponseParser _dgcgResponseParser;
        private readonly IUkGatewayService _ukGatewayService;
        private readonly IEuCertificateRepository _euCertificateRepository;
        private readonly IMetricLogService _metricLogService;

        public UpdateEuCertificateRepositoryTask(ILogger<UpdateEuCertificateRepositoryTask> logger, CronOptions cronOptions,
            FeatureToggles featureToggles, IDgcgService dgcgService, IDgcgResponseParser dgcgResponseParser,
            IUkGatewayService ukGatewayService, IEuCertificateRepository euCertificateRepository, IMetricLogService metricLogService)
        {
            _logger = logger;
            _cronOptions = cronOptions;
            _featureToggles = featureToggles;
            _dgcgService = dgcgService;
            _dgcgResponseParser = dgcgResponseParser;
            _ukGatewayService = ukGatewayService;
            _euCertificateRepository = euCertificateRepository;
            _metricLogService = metricLogService;
        }

        public void SetupTask()
        {
            _logger.LogInformation("Adding task 'UpdateEuCertificateRepository' {CronString}", _cronOptions.UpdateEuCertificateRepositoryCron);
            RecurringJob.AddOrUpdate("update-eu-certificate-repo", () => UpdateEuCertificateRepository(), _cronOptions.UpdateEuCertificateRepositoryCron);
            _logger.LogInformation($"Scheduling update-eu-certificate-repo on startup after {_cronOptions.ScheduleUpdateEuCertificateRepositoryOnStartupAfterSeconds} seconds");
            BackgroundJob.Schedule(() => UpdateEuCertificateRepository(),
                TimeSpan.FromSeconds(_cronOptions.ScheduleUpdateEuCertificateRepositoryOnStartupAfterSeconds));
        }

        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [DisableConcurrentExecution(DisableConcurrentTimeout)]
        public async Task UpdateEuCertificateRepository()
        {
            var failure = false;

            try
            {
                var trustlistResponse = await _dgcgService.GetTrustListAsync();
                var euDocSignerCertificates = _dgcgResponseParser.ParseToEuDocSignerCertificate(trustlistResponse);

                var cleanupOptions = _featureToggles.UseUkGateway
                    ? CleanupWhichCertificates.AllButUkCertificates
                    : CleanupWhichCertificates.All;
                await _euCertificateRepository.CleanupAndPersistEuDocSignerCertificates(euDocSignerCertificates, cleanupOptions);

                _metricLogService.AddMetric("RetrieveEuCertificates_Success", true);
            }
            catch (GeneralDgcgFaultException e)
            {
                failure = true;
                _logger.LogError(e, "FaultException caught"); 
                _metricLogService.AddMetric("RetrieveEuCertificates_Success", false);
            }
            catch (Exception e)
            {
                failure = true;
                _logger.LogError(e, "UpdateEuCertificateRepository fails");
                _metricLogService.AddMetric("RetrieveEuCertificates_Success", false);
            }

            if (_featureToggles.UseUkGateway)
            {
                try
                {
                    var ukCertificates = await _ukGatewayService.GetTrustListAsync();

                    await _euCertificateRepository.CleanupAndPersistEuDocSignerCertificates(ukCertificates, CleanupWhichCertificates.UkCertificates);

                    _metricLogService.AddMetric("RetrieveUkCertificates_Success", true);
                }
                catch (Exception e)
                {
                    failure = true;
                    _logger.LogError(e, "UpdateEuCertificateRepository fails");
                    _metricLogService.AddMetric("RetrieveUkCertificates_Success", false);
                }
            }

            try
            {
                if (failure)
                {
                    throw new InvalidOperationException("Either EU or UK integration failed. Debug via additional logs");
                }

                _metricLogService.AddMetric("UpdateEuCertificateRepository_Success", true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "UpdateEuCertificateRepository fails");
                _metricLogService.AddMetric("UpdateEuCertificateRepository_Success", false);
                throw;
            }
            finally
            {
                _metricLogService.DumpMetricsToLog("UpdateEuCertificateRepository finished");
            }
        }
    }
}