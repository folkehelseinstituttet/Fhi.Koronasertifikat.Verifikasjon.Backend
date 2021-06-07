using System.ComponentModel.DataAnnotations;

namespace FHICORC.Application.Models.Options
{
    public class ConnectionStringOptions
    {
        [Required]
        public string PgsqlDatabase { get; set; }

        public string HangfirePgsqlDatabase { get; set; }
    }
}