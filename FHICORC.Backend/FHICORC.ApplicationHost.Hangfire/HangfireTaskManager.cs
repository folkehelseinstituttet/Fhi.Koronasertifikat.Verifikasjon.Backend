using Hangfire;
using Hangfire.Storage;
using FHICORC.ApplicationHost.Hangfire.Interfaces;

namespace FHICORC.ApplicationHost.Hangfire
{
    public class HangfireTaskManager : IHangfireTaskManager
    {
        private readonly IUpdateCertificateRepositoryTask _updateCertificateRepositoryTask;
        private readonly ICountriesReportRepositoryTask _coutriesReportRepositoryTask;
        private readonly IUpdateRevocationListTask _updateRevocationListTask;

        public HangfireTaskManager(IUpdateCertificateRepositoryTask updateCertificateRepositoryTask, ICountriesReportRepositoryTask coutriesReportRepositoryTask, IUpdateRevocationListTask updateRevocationListTask)
        {
            _updateCertificateRepositoryTask = updateCertificateRepositoryTask;
            _coutriesReportRepositoryTask = coutriesReportRepositoryTask;
            _updateRevocationListTask = updateRevocationListTask;
        }

        public void SetupHangfireTasks()
        {
            using var connection = JobStorage.Current.GetConnection();
            connection.GetRecurringJobs().ForEach(j => RecurringJob.RemoveIfExists(j.Id));

            _updateCertificateRepositoryTask.SetupTask();
            _coutriesReportRepositoryTask.SetupTask();
            _updateRevocationListTask.SetupTask();
        }
    }
}