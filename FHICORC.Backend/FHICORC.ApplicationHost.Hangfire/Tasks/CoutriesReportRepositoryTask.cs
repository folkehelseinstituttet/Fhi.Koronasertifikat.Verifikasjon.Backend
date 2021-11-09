using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.ApplicationHost.Hangfire.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace FHICORC.ApplicationHost.Hangfire.Tasks
{
    public class CountriesReportRepositoryTask : ICountriesReportRepositoryTask
    {
        // This cannot be moved to AppSettings as it is used in attribute and therefore must be constant.
        public const int DisableConcurrentTimeout = 10;

        private readonly ILogger<CountriesReportRepositoryTask> _logger;
        private readonly CronOptions _cronOptions;
        private readonly MailOptions _mailOptions;
        private readonly ICountriesReportRepository _countriesReportRepository;

        public CountriesReportRepositoryTask(ILogger<CountriesReportRepositoryTask> logger, CronOptions cronOptions,
               ICountriesReportRepository countriesReportRepository, 
               MailOptions mailOptions)
        {
            _logger = logger;
            _cronOptions = cronOptions;
            _mailOptions = mailOptions;
            _countriesReportRepository = countriesReportRepository;
        }

        public void SetupTask()
        {
            _logger.LogInformation("Adding task 'CountriesReportRepository' ", _cronOptions.CountriesReportRepositoryCron);
            RecurringJob.AddOrUpdate("countries-report-repo", () => CoutriesReportRepository(), _cronOptions.CountriesReportRepositoryCron);
            _logger.LogInformation($"Scheduling countries-report-repo on startup after {_cronOptions.CountriesReportRepositoryOnStartupAfterSeconds} seconds");
            BackgroundJob.Schedule(() => CoutriesReportRepository(),
                TimeSpan.FromSeconds(_cronOptions.CountriesReportRepositoryOnStartupAfterSeconds));
        }

        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [DisableConcurrentExecution(DisableConcurrentTimeout)]
        public async Task CoutriesReportRepository()
        {
            try
            {
                _logger.LogInformation("Countries Report start task");
                var list = await _countriesReportRepository.GetAllCountries();

                _logger.LogInformation("Send email start task");
                if (SendEmail(list, _mailOptions).Result == false)
                {
                    _logger.LogError("Send email fails");
                }
            }

            catch (Exception e)
            {
                _logger.LogError(e, "CoutriesReportRepository fails");
            }
        }

        private async Task<bool> SendEmail(IList<string> list, MailOptions mailOptions)
        {
            var client = new SendGridClient(_mailOptions.APIKey);
            var from = new EmailAddress(_mailOptions.From, _mailOptions.FromUser);
            var subject = String.Format(_mailOptions.Subject + " " + DateTime.Now.ToShortDateString());

            var toList = _mailOptions.To.Split(';');
            var toUserList = _mailOptions.ToUser.Split(';');
            var to = new List<EmailAddress>();
            if (toList.Length == toUserList.Length && toList.Length > 0)
            {
                for(int i = 0; i < toList.Length; i++)
                {
                     to.Add(new EmailAddress(toList[i],toUserList[i]));   
                }
            }
            else
            {
                _logger.LogError("Recipient list empty or recipient list not corespondent to recipient names.");
                return false;
            }
            

            string plainTextContent, htmlContent;

            if (list.Count == 3)
            {
                plainTextContent = "The report contains a list of countries with valid public keys. The report is generated daily and also contains information about countries added to the list or removed from the list the past 24 hours\n\n" +
                                   $"All countries:\n{list[0]}\n" +
                                   $"Added:\n{list[1]}\n" +
                                   $"Removed:\n{list[2]}";

                htmlContent = "The report contains a list of countries with valid public keys. The report is generated daily and also contains information about countries added to the list or removed from the list the past 24 hours <br><br>" +
                              $"All countries:<br>{list[0]}<br>" +
                              $"Added:<br>{list[1]}<br>" +
                              $"Removed:<br>{list[2]}";
            }
            else
            {
                _logger.LogError("List count != 3");
                return false;
            }

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, to, subject, plainTextContent, htmlContent);
            var res =  await client.SendEmailAsync(msg);
            return true;
        }
    }
}