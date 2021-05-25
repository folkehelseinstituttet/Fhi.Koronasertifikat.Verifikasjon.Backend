using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using FHICORC.Application.Common.Logging.Metrics;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.ApplicationHost.Hangfire.Interfaces;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;

namespace FHICORC.ApplicationHost.Hangfire.Tasks
{
    public class UpdateEuCertificateRepositoryTask : IUpdateEuCertificateRepositoryTask
    {
        // This cannot be moved to AppSettings as it is used in attribute and therefore must be constant.
        public const int DisableConcurrentTimeout = 20;

        private readonly ILogger<UpdateEuCertificateRepositoryTask> _logger;
        private readonly CronOptions _cronOptions;
        private readonly IDgcgService _dgcgService;
        private readonly IDgcgResponseParser _dgcgResponseParser;
        private readonly IEuCertificateRepository _euCertificateRepository;

        public UpdateEuCertificateRepositoryTask(ILogger<UpdateEuCertificateRepositoryTask> logger, CronOptions cronOptions,
            IDgcgService dgcgService, IDgcgResponseParser dgcgResponseParser,
            IEuCertificateRepository euCertificateRepository)
        {
            _logger = logger;
            _cronOptions = cronOptions;
            _dgcgService = dgcgService;
            _dgcgResponseParser = dgcgResponseParser;
            _euCertificateRepository = euCertificateRepository;
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
            try
            {
                var trustlistResponse = await _dgcgService.GetTrustListAsync();
                var euDocSignerCertificates = _dgcgResponseParser.ParseToEuDocSignerCertificate(trustlistResponse);
                await _euCertificateRepository.BulkUpsertEuDocSignerCertificates(euDocSignerCertificates);
                
                _logger.MetricLogMessage("UpdateEuCertificateRepository Success");
            }
            catch (Exception e)
            {
                _logger.LogWarning("UpdateEuCertificateRepository fails", e.StackTrace);
                throw;
            }
        }
    }
}