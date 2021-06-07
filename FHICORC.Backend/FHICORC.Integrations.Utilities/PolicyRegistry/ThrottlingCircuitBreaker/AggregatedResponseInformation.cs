namespace FHICORC.Integrations.Utilities.PolicyRegistry.ThrottlingCircuitBreaker
{
    public class AggregatedResponseInformation
    {
        public double ErrorPercentage { get; set; }

        public long AverageExecutionTimeMs { get; set; }

        public long Calls { get; set; }

        public long CallsIncludingThrottled { get; set; }
    }
}