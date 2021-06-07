using FHICORC.Application.Models.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace FHICORC.Application.Models.Options
{
    public class ThrottlingCircuitBreakerOptions
    {
        [Required]
        [Range(10, 30000,
            ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int SamplingIntervalMs { get; set; }

        [Required]
        [Range(1, 10000,
            ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int MinCallsPerSamplingInterval { get; set; }

        [GreaterThanProperty("MinCallsPerSamplingInterval", false)]
        public int? MaxCallsPerSamplingInterval { get; set; }

        [Required]
        [Range(0, 100,
            ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public double ThrottleStartErrorPct { get; set; }

        [Required]
        [GreaterThanProperty("ThrottleStartErrorPct", true)]
        [Range(0, 100,
            ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public double ThrottleAllErrorPct { get; set; }

        [Range(0, int.MaxValue,
            ErrorMessage = "Value for {0} must be positive.")]
        public int? ThrottleStartResponseTimeMs { get; set; }

        [GreaterThanProperty("ThrottleStartResponseTimeMs", true)]
        public int? ThrottleAllResponseTimeMs { get; set; }
    }
}