using Hangfire;
using Hangfire.Storage;
using FHICORC.ApplicationHost.Hangfire.Interfaces;

namespace FHICORC.ApplicationHost.Hangfire
{
    public class HangfireTaskManager : IHangfireTaskManager
    {
        private readonly IUpdateEuCertificateRepositoryTask _updateEuCertificateRepositoryTask;

        public HangfireTaskManager(IUpdateEuCertificateRepositoryTask updateEuCertificateRepositoryTask)
        {
            _updateEuCertificateRepositoryTask = updateEuCertificateRepositoryTask;
        }

        public void SetupHangfireTasks()
        {
            using var connection = JobStorage.Current.GetConnection();
            connection.GetRecurringJobs().ForEach(j => RecurringJob.RemoveIfExists(j.Id));

            _updateEuCertificateRepositoryTask.SetupTask();
        }
    }
}