using Hangfire;
using Hangfire.Storage;
using FHICORC.ApplicationHost.Hangfire.Interfaces;

namespace FHICORC.ApplicationHost.Hangfire
{
    public class HangfireTaskManager : IHangfireTaskManager
    {
        private readonly IUpdateCertificateRepositoryTask _updateCertificateRepositoryTask;
        private readonly ICountriesReportRepositoryTask _coutriesReportRepositoryTask;
        private readonly ISmartHealthCardIssuersTask _smartHealthCardIssuersTask;
        private readonly ISmartHealthCardVaccinesTask _smartHealthCardVaccinesTask;

        public HangfireTaskManager(
            IUpdateCertificateRepositoryTask updateCertificateRepositoryTask,
            ICountriesReportRepositoryTask coutriesReportRepositoryTask,
            ISmartHealthCardIssuersTask smartHealthCardIssuersTask,
            ISmartHealthCardVaccinesTask smartHealthCardVaccinesTask)
        {
            _updateCertificateRepositoryTask = updateCertificateRepositoryTask;
            _coutriesReportRepositoryTask = coutriesReportRepositoryTask;
            _smartHealthCardIssuersTask = smartHealthCardIssuersTask;
            _smartHealthCardVaccinesTask = smartHealthCardVaccinesTask;
        }

        public void SetupHangfireTasks()
        {
            using var connection = JobStorage.Current.GetConnection();
            connection.GetRecurringJobs().ForEach(j => RecurringJob.RemoveIfExists(j.Id));

            _updateCertificateRepositoryTask.SetupTask();
            _coutriesReportRepositoryTask.SetupTask();
            _smartHealthCardIssuersTask.SetupTask();
            _smartHealthCardVaccinesTask.SetupTask();
        }
    }
}