using System.ComponentModel.DataAnnotations;

namespace FHICORC.Application.Models.Options
{
    public class ValueSetOptions
    {
        [Required]
        public string ValueSetsDirectory { get; set; }
    }
}
