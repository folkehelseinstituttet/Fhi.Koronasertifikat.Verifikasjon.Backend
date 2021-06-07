using System.ComponentModel.DataAnnotations;

namespace FHICORC.Application.Models.Options
{
    public class TextOptions
    {
        [Required]
        public string TextsDirectory { get; set; }
    }
}
