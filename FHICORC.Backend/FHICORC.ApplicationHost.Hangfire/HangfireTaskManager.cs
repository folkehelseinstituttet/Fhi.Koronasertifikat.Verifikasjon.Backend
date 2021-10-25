using Hangfire;
using Hangfire.Storage;
using FHICORC.ApplicationHost.Hangfire.Interfaces;

namespace FHICORC.ApplicationHost.Hangfire
{
    public class HangfireTaskManager : IHangfireTaskManager
    {
        private readonly IUpdateCertificateRepositoryTask _updateCertificateRepositoryTask;
        private readonly ICountriesReportRepositoryTask _coutriesReportRepositoryTask;

        public HangfireTaskManager(IUpdateCertificateRepositoryTask updateCertificateRepositoryTask, ICountriesReportRepositoryTask coutriesReportRepositoryTask)
        {
            _updateCertificateRepositoryTask = updateCertificateRepositoryTask;
            _coutriesReportRepositoryTask = coutriesReportRepositoryTask;
        }

        public void SetupHangfireTasks()
        {
            using var connection = JobStorage.Current.GetConnection();
            connection.GetRecurringJobs().ForEach(j => RecurringJob.RemoveIfExists(j.Id));

            _updateCertificateRepositoryTask.SetupTask();
            _coutriesReportRepositoryTask.SetupTask();
        }
    }
}