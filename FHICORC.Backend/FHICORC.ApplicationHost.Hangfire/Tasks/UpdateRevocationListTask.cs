using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using FHICORC.Application.Common.Interfaces;
using FHICORC.Application.Models.Options;
using FHICORC.ApplicationHost.Hangfire.Interfaces;
using FHICORC.Integrations.DGCGateway;
using FHICORC.Integrations.DGCGateway.Services.Interfaces;
using FHICORC.Application.Models;
using FHICORC.Integrations.DGCGateway.Services;
using Hangfire.Storage;
using System.Linq;

namespace FHICORC.ApplicationHost.Hangfire.Tasks
{
    public class UpdateRevocationListTask : IUpdateRevocationListTask
    {
        // This cannot be moved to AppSettings as it is used in attribute and therefore must be constant.
        public const int DisableConcurrentTimeout = 10;

        private readonly ILogger<UpdateCertificateRepositoryTask> _logger;
        private readonly CronOptions _cronOptions;
        private readonly IDgcgService _dgcgService;
        private readonly IMetricLogService _metricLogService;
        private readonly IDGCGRevocationService _revocationService;
        //private readonly ISeedDbService _seedDbService;
        private readonly HangfireContext _hangfireContext;

        public UpdateRevocationListTask(ILogger<UpdateCertificateRepositoryTask> logger, CronOptions cronOptions,
            IDgcgService dgcgService,
            IMetricLogService metricLogService,
            IDGCGRevocationService revocationService,
            HangfireContext hangfireContext)
        {
            _logger = logger;
            _cronOptions = cronOptions;
            _dgcgService = dgcgService;
            _metricLogService = metricLogService;
            _revocationService = revocationService;
            _hangfireContext = hangfireContext;

            //_seedDbService = seedDbService;

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

            var api = JobStorage.Current.GetMonitoringApi();
            var succeededJobs = api.SucceededJobs(0, int.MaxValue);
            var lastSucceedetDate = succeededJobs.FirstOrDefault(s => s.Value.Job.Type.Name == "UpdateRevocationListTask").Value?.SucceededAt;
           
            try
            {
                var modifiedSince = lastSucceedetDate != null ? lastSucceedetDate.ToString() : "2021-06-01T00:00:00Z";
                revocationBatchList = await _dgcgService.GetRevocationBatchListAsync(modifiedSince);
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
                await _revocationService.PopulateRevocationDatabase(revocationBatchList);

                //TRUNCATE public."RevocationBatch", public."RevocationSuperFilter" CASCADE;
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