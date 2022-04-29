using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using FHICORC.Application.Common.Interfaces;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Repositories.Enums;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.ApplicationHost.Hangfire.Interfaces;
using FHICORC.Integrations.DGCGateway;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using FHICORC.Integrations.UkGateway.Services.Interfaces;

namespace FHICORC.ApplicationHost.Hangfire.Tasks
{
    public class UpdateCertificateRepositoryTask : IUpdateCertificateRepositoryTask
    {
        // This cannot be moved to AppSettings as it is used in attribute and therefore must be constant.
        public const int DisableConcurrentTimeout = 10;

        private readonly ILogger<UpdateCertificateRepositoryTask> _logger;
        private readonly CronOptions _cronOptions;
        private readonly FeatureToggles _featureToggles;
        private readonly IDgcgService _dgcgService;
        private readonly IDgcgResponseParser _dgcgResponseParser;
        private readonly IUkGatewayService _ukGatewayService;
        private readonly IEuCertificateRepository _euCertificateRepository;
        private readonly IMetricLogService _metricLogService;

        public UpdateCertificateRepositoryTask(ILogger<UpdateCertificateRepositoryTask> logger, CronOptions cronOptions,
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
            _logger.LogInformation("Adding task 'UpdateRevocationListTask' {CronString}", _cronOptions.UpdateEuCertificateRepositoryCron);
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

                var cleanupOptions = CleanupWhichCertificates.AllButUkCertificates;
                if (!_featureToggles.UseUkGateway)
                {
                    cleanupOptions |= CleanupWhichCertificates.UkCertificates;
                }
                if (!_featureToggles.UseNiGateway)
                {
                    cleanupOptions |= CleanupWhichCertificates.UkNiCertificates;
                }
                if (!_featureToggles.UseScGateway)
                {
                    cleanupOptions |= CleanupWhichCertificates.UkScCertificates;
                }

                var a = await _euCertificateRepository.CleanupAndPersistEuDocSignerCertificates(euDocSignerCertificates, cleanupOptions);

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
                    var ukCertificates = await _ukGatewayService.GetTrustListAsync(SpecialCountryCodes.UK);

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

            if (_featureToggles.UseNiGateway)
            {
                try
                {
                    var niCertificates = await _ukGatewayService.GetTrustListAsync(SpecialCountryCodes.UK_NI);

                    await _euCertificateRepository.CleanupAndPersistEuDocSignerCertificates(niCertificates, CleanupWhichCertificates.UkNiCertificates);

                    _metricLogService.AddMetric("RetrieveNiCertificates_Success", true);
                }
                catch (Exception e)
                {
                    failure = true;
                    _logger.LogError(e, "UpdateEuCertificateRepository fails");
                    _metricLogService.AddMetric("RetrieveNiCertificates_Success", false);
                }
            }

            if (_featureToggles.UseScGateway)
            {
                try
                {
                    var scCertificates = await _ukGatewayService.GetTrustListAsync(SpecialCountryCodes.UK_SC);

                    await _euCertificateRepository.CleanupAndPersistEuDocSignerCertificates(scCertificates, CleanupWhichCertificates.UkScCertificates);

                    _metricLogService.AddMetric("RetrieveScCertificates_Success", true);
                }
                catch (Exception e)
                {
                    failure = true;
                    _logger.LogError(e, "UpdateEuCertificateRepository fails");
                    _metricLogService.AddMetric("RetrieveScCertificates_Success", false);
                }
            }

            try
            {
                if (failure)
                {
                    throw new InvalidOperationException("Either EU, UK, NI or SC integration failed. Debug via additional logs");
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