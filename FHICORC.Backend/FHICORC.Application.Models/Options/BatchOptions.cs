using System.ComponentModel.DataAnnotations;

namespace FHICORC.Application.Models.Options
{
    public class BatchOptions
    {
        public int BatchSize { get; set; }
        public string CountryCode { get; set; }
        public string HashType { get; set; }
    }
}
