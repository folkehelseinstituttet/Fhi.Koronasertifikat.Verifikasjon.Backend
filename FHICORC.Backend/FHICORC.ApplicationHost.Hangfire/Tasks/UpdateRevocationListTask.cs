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
using FHICORC.Application.Models;

namespace FHICORC.ApplicationHost.Hangfire.Tasks
{
    public class UpdateRevocationListTask : IUpdateRevocationListTask
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

        public UpdateRevocationListTask(ILogger<UpdateCertificateRepositoryTask> logger, CronOptions cronOptions,
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
            _logger.LogInformation("Adding task 'UpdateRevocationList' {CronString}", _cronOptions.UpdateRevocationListTaskCron);
            RecurringJob.AddOrUpdate("update-revocation-list", () => UpdateRevocationList(), _cronOptions.UpdateEuCertificateRepositoryCron);
            _logger.LogInformation($"Scheduling update-revocation-list on startup after {_cronOptions.ScheduleUpdateRevocationListTaskOnStartupAfterSeconds} seconds");
            BackgroundJob.Schedule(() => UpdateRevocationList(),
                TimeSpan.FromSeconds(_cronOptions.ScheduleUpdateRevocationListTaskOnStartupAfterSeconds));
        }

        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [DisableConcurrentExecution(DisableConcurrentTimeout)]
        public async Task UpdateRevocationList()
        {
            var failure = false;
            DgcgRevocationBatchListRespondDto revocationBatchList = new DgcgRevocationBatchListRespondDto();


            try
            {
                //revocationBatchList = await _dgcgService.GetRevocationBatchListAsync();
                _metricLogService.AddMetric("RetrieveRevocationBatchList_Success", true);
            }
            catch (GeneralDgcgFaultException e)
            {
                failure = true;
                _logger.LogError(e, "FaultException caught"); 
                _metricLogService.AddMetric("RetrieveRevocationBatchList_Success", false);
            }
            catch (Exception e)
            {
                failure = true;
                _logger.LogError(e, "UpdateRevocationList fails");
                _metricLogService.AddMetric("RetrieveRevocationBatchList_Success", false);
            }


            try
            {
                var batchId = "699978cf-d2d4-4093-8b54-ab2cf695d76d";
                //revocationBatchList.Batches[0].BatchId;

                var revocationHashList = await _dgcgService.GetRevocationBatchAsync(batchId);

                _metricLogService.AddMetric("RetrieveRevocationBatch_Success", true);


            }
            catch (GeneralDgcgFaultException e)
            {
                failure = true;
                _logger.LogError(e, "FaultException caught");
                _metricLogService.AddMetric("RetrieveRevocationBatch_Success", false);
            }
            catch (Exception e)
            {
                failure = true;
                _logger.LogError(e, "UpdateRevocationList fails");
                _metricLogService.AddMetric("RetrieveRevocationBatch_Success", false);
            }



            try
            {
                if (failure)
                {
                    throw new InvalidOperationException("One of the RevocationList Updating task failed. Debug via additional logs");
                }

                _metricLogService.AddMetric("UpdateRevocationList_Success", true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "UpdateRevocationList fails");
                _metricLogService.AddMetric("UpdateRevocationList_Success", false);
                throw;
            }
            finally
            {
                _metricLogService.DumpMetricsToLog("UpdateRevocationList_Success finished");
            }
        }
    }
}