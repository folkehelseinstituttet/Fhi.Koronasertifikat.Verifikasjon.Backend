using System.ComponentModel.DataAnnotations;

namespace FHICORC.Application.Models.Options
{
    public class BloomBucketOptions
    {
        [Required]
        public int NumberOfBuckets { get; set; }
        [Required]
        public int MinValue { get; set; }
        [Required]
        public int MaxValue { get; set; }
        [Required]
        public double FalsePositiveProbability { get; set; }

        [Required]
        public int ExpieryDateLeewayInDays { get; set; }

    }
}
