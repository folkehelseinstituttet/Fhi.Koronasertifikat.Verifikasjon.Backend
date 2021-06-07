using System.ComponentModel.DataAnnotations;

namespace FHICORC.Application.Models.Options
{
    public class CacheOptions
    {
        [Required]
        public int AbsoluteExpiration { get; set; }
        [Required]
        public int SlidingExpiration { get; set; }
        [Required]
        public int CacheSize { get; set; }
    }
}
