using Polly;
using FHICORC.Application.Models.Options;

namespace FHICORC.Integrations.Utilities.PolicyRegistry.ThrottlingCircuitBreaker
{
    public static class PolicyBuilderExtensions
    {
        public static ThrottlingCircuitBreakerPolicy ThrottlingCircuitBreaker(
            this PolicyBuilder policyBuilder, ThrottlingCircuitBreakerOptions options)
        {
            return new ThrottlingCircuitBreakerPolicy(policyBuilder, options);
        }

        public static ThrottlingCircuitBreakerPolicy<TResult> ThrottlingCircuitBreaker<TResult>(
            this PolicyBuilder<TResult> policyBuilder, ThrottlingCircuitBreakerOptions options)
        {
            return new ThrottlingCircuitBreakerPolicy<TResult>(policyBuilder, options);
        }

        public static AsyncThrottlingCircuitBreakerPolicy ThrottlingCircuitBreakerAsync(
            this PolicyBuilder policyBuilder, ThrottlingCircuitBreakerOptions options)
        {
            return new AsyncThrottlingCircuitBreakerPolicy(policyBuilder, options);
        }

        public static AsyncThrottlingCircuitBreakerPolicy<TResult> ThrottlingCircuitBreakerAsync<TResult>(
            this PolicyBuilder<TResult> policyBuilder, ThrottlingCircuitBreakerOptions options)
        {
            return new AsyncThrottlingCircuitBreakerPolicy<TResult>(policyBuilder, options);
        }
    }
}
