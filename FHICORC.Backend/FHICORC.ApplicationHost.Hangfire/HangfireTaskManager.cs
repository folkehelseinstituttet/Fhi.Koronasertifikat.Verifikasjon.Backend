using Hangfire;
using Hangfire.Storage;
using FHICORC.ApplicationHost.Hangfire.Interfaces;

namespace FHICORC.ApplicationHost.Hangfire
{
    public class HangfireTaskManager : IHangfireTaskManager
    {
        private readonly IUpdateCertificateRepositoryTask _updateCertificateRepositoryTask;

        public HangfireTaskManager(IUpdateCertificateRepositoryTask updateCertificateRepositoryTask)
        {
            _updateCertificateRepositoryTask = updateCertificateRepositoryTask;
        }

        public void SetupHangfireTasks()
        {
            using var connection = JobStorage.Current.GetConnection();
            connection.GetRecurringJobs().ForEach(j => RecurringJob.RemoveIfExists(j.Id));

            _updateCertificateRepositoryTask.SetupTask();
        }
    }
}