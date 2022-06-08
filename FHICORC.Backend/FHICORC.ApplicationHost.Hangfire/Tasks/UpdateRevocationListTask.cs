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
using FHICORC.Infrastructure.Database.Context;
using FHICORC.Domain.Models.Revocation;
using Microsoft.EntityFrameworkCore;

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
        private readonly CoronapassContext _coronapassContext;

        public UpdateRevocationListTask(ILogger<UpdateCertificateRepositoryTask> logger, CronOptions cronOptions,
            IDgcgService dgcgService,
            IMetricLogService metricLogService,
            IDGCGRevocationService revocationService,
            CoronapassContext coronapassContext)
        {
            _logger = logger;
            _cronOptions = cronOptions;
            _dgcgService = dgcgService;
            _metricLogService = metricLogService;
            _revocationService = revocationService;
            _coronapassContext = coronapassContext;
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
                var revocationDownloadJobSucceeded = _coronapassContext.RevocationDownloadJobSucceeded.FirstOrDefault();

                DateTime modifiedSince;

                if (revocationDownloadJobSucceeded is null)
                    modifiedSince = new DateTime(2021, 6, 1, 0, 0, 0, DateTimeKind.Utc);
                else
                    modifiedSince = revocationDownloadJobSucceeded.LastDownloadJobSucceeded;


                revocationBatchList = await _dgcgService.GetRevocationBatchListAsync(modifiedSince);

                if (revocationDownloadJobSucceeded is null)
                    _coronapassContext.Add(new RevocationDownloadJobSucceeded(DateTime.UtcNow));
                else
                {
                    revocationDownloadJobSucceeded.LastDownloadJobSucceeded = DateTime.UtcNow;
                    _coronapassContext.Entry(revocationDownloadJobSucceeded).State = EntityState.Modified;
                }

                _coronapassContext.SaveChanges();
                
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