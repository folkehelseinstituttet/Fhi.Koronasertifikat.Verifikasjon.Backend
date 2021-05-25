using System.ComponentModel.DataAnnotations;

namespace FHICORC.Application.Models.Options
{
    public class HangfireHealthOptions
    {
        [Required]
        public int MaximumJobsFailed { get; set; }
        [Required]
        public int MinimumAvailableServers { get; set; }
    }
}
