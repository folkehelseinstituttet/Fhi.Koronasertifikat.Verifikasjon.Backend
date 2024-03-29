﻿using System.ComponentModel.DataAnnotations;

namespace FHICORC.Application.Models.Options
{
    public class CronOptions
    {
        [Required]
        public string UpdateEuCertificateRepositoryCron { get; set; }
        [Required]
        public int ScheduleUpdateEuCertificateRepositoryOnStartupAfterSeconds { get; set; }
        [Required]
        public string CountriesReportRepositoryCron { get; set; }
        [Required]
        public int CountriesReportRepositoryOnStartupAfterSeconds { get; set; }
    }
}
