using Polly;
using FHICORC.Application.Models.Options;
using System;
using System.Threading;

namespace FHICORC.Integrations.Utilities.PolicyRegistry.ThrottlingCircuitBreaker
{
    public class ThrottlingCircuitBreakerPolicy : Policy
    {
        private readonly ThrottlingCircuitBreakerImplementation _implementation;

        public ThrottlingCircuitBreakerPolicy(PolicyBuilder policyBuilder, ThrottlingCircuitBreakerOptions options) : base(policyBuilder)
        {
            _implementation = new ThrottlingCircuitBreakerImplementation(options);
        }

        public AggregatedResponseInformation AggregatedInformation => _implementation.AggregatedInformation;
        public double CurrentThrottle => _implementation.CurrentThrottle;

        protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            return _implementation.Implementation(action, context, ExceptionPredicates, null, cancellationToken);
        }
    }

    public class ThrottlingCircuitBreakerPolicy<TResult> : Policy<TResult>
    {
        private readonly ThrottlingCircuitBreakerImplementation _implementation;

        public ThrottlingCircuitBreakerPolicy(PolicyBuilder<TResult> policyBuilder, ThrottlingCircuitBreakerOptions options) : base(policyBuilder)
        {
            _implementation = new ThrottlingCircuitBreakerImplementation(options);
        }

        public AggregatedResponseInformation AggregatedInformation => _implementation.AggregatedInformation;
        public double CurrentThrottle => _implementation.CurrentThrottle;

        protected override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
        {
            return _implementation.Implementation(action, context, ExceptionPredicates, ResultPredicates, cancellationToken);
        }
    }
}
