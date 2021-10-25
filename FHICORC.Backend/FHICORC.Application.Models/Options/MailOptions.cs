using System.ComponentModel.DataAnnotations;

namespace FHICORC.Application.Models.Options
{
    public class MailOptions
    {
        [Required]
        public string From { get; set; }
        [Required]
        public string FromUser { get; set; }
        [Required]
        public string To { get; set; }
        [Required]
        public string ToUser { get; set; }
        [Required]
        public string APIKey { get; set; }
        [Required]
        public string Subject { get; set; }

    }
}
