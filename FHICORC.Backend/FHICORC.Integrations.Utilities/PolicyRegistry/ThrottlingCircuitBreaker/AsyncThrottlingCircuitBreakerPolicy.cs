using Polly;
using FHICORC.Application.Models.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FHICORC.Integrations.Utilities.PolicyRegistry.ThrottlingCircuitBreaker
{
    public class AsyncThrottlingCircuitBreakerPolicy : AsyncPolicy
    {
        private readonly ThrottlingCircuitBreakerImplementation _implementation;

        public AsyncThrottlingCircuitBreakerPolicy(PolicyBuilder policyBuilder, ThrottlingCircuitBreakerOptions options) : base(policyBuilder)
        {
            _implementation = new ThrottlingCircuitBreakerImplementation(options);
        }

        public AggregatedResponseInformation AggregatedInformation => _implementation.AggregatedInformation;
        public double CurrentThrottle => _implementation.CurrentThrottle;

        protected override async Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action,
            Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return await _implementation.ImplementationAsync(action, context, continueOnCapturedContext, ExceptionPredicates, null, cancellationToken);
        }
    }

    public class AsyncThrottlingCircuitBreakerPolicy<TResult> : AsyncPolicy<TResult>
    {
        private readonly ThrottlingCircuitBreakerImplementation _implementation;

        public AsyncThrottlingCircuitBreakerPolicy(PolicyBuilder<TResult> policyBuilder, ThrottlingCircuitBreakerOptions options) : base(policyBuilder)
        {
            _implementation = new ThrottlingCircuitBreakerImplementation(options);
        }

        public AggregatedResponseInformation AggregatedInformation => _implementation.AggregatedInformation;
        public double CurrentThrottle => _implementation.CurrentThrottle;

        protected override async Task<TResult> ImplementationAsync(Func<Context, CancellationToken, Task<TResult>> action,
            Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return await _implementation.ImplementationAsync(action, context, continueOnCapturedContext, ExceptionPredicates, ResultPredicates, cancellationToken);
        }
    }
}
