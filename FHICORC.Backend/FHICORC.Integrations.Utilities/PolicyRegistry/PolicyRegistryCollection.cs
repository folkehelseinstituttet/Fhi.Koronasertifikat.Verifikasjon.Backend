using Polly;
using FHICORC.Application.Models.Options;
using FHICORC.Integrations.Utilities.PolicyRegistry.ThrottlingCircuitBreaker;
using System;

namespace FHICORC.Integrations.Utilities
{
    public static class PolicyRegistryCollection
    {
        public static AsyncPolicy CircuitBreakerPolicyCbs(ThrottlingCircuitBreakerOptions throttlingCircuitBreakerOptions)
        {
            return Policy.Handle<Exception>().ThrottlingCircuitBreakerAsync(throttlingCircuitBreakerOptions);
        }

        public static AsyncPolicy CircuitBreakerPolicyIdws(ThrottlingCircuitBreakerOptions throttlingCircuitBreakerOptions)
        {
            return Policy.Handle<Exception>().ThrottlingCircuitBreakerAsync(throttlingCircuitBreakerOptions);
        }
    }
}
