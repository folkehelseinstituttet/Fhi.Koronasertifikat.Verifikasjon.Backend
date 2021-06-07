using Polly;
using Polly.CircuitBreaker;
using FHICORC.Application.Models.Options;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FHICORC.Integrations.Utilities.PolicyRegistry.ThrottlingCircuitBreaker
{
    internal class ThrottlingCircuitBreakerImplementation
    {
        private readonly ResponseAggregator _aggregator;
        private readonly ThrottlingCircuitBreakerOptions _options;

        public ThrottlingCircuitBreakerImplementation(ThrottlingCircuitBreakerOptions options)
        {
            _options = options;
            _aggregator = new ResponseAggregator(_options.SamplingIntervalMs);
        }

        public AggregatedResponseInformation AggregatedInformation => _aggregator.AggregatedInformation;

        public double CurrentThrottle
        {
            get
            {
                var aggregatedData = AggregatedInformation;
                var errorRange = _options.ThrottleAllErrorPct - _options.ThrottleStartErrorPct;
                var throttleErrors = Math.Min(errorRange,
                    Math.Max(0, aggregatedData.ErrorPercentage - _options.ThrottleStartErrorPct)) / errorRange;

                double throttle;
                if (_options.ThrottleStartResponseTimeMs != null && _options.ThrottleAllResponseTimeMs != null)
                {
                    var respTimeRange = _options.ThrottleAllResponseTimeMs.Value -
                                        _options.ThrottleStartResponseTimeMs.Value;
                    var throttleResponseTime = Math.Min((double)respTimeRange,
                        Math.Max(0, aggregatedData.AverageExecutionTimeMs - _options.ThrottleStartResponseTimeMs.Value)) / respTimeRange;

                    throttle = Math.Max(throttleErrors, throttleResponseTime);
                }
                else
                {
                    throttle = throttleErrors;
                }

                return throttle;
            }
        }


        public async Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context,
            bool continueOnCapturedContext, ExceptionPredicates exceptionPredicates,
            ResultPredicates<TResult> resultPredicates, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var execution = Execution.Begin(this);
            try
            {
                return execution.Result(resultPredicates, await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext));
            }
            catch (Exception ex)
            {
                execution.Error(exceptionPredicates, ex);
                throw;
            }
            finally
            {
                execution.Finish();
            }
        }

        public TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context,
           ExceptionPredicates exceptionPredicates, ResultPredicates<TResult> resultPredicates, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var execution = Execution.Begin(this);
            try
            {
                return execution.Result(resultPredicates, action(context, cancellationToken));
            }
            catch (Exception ex)
            {
                execution.Error(exceptionPredicates, ex);
                throw;
            }
            finally
            {
                execution.Finish();
            }
        }

        private class Execution
        {
            private static readonly double TickMsFrequency = 1000D / Stopwatch.Frequency;

            private readonly ThrottlingCircuitBreakerImplementation _implementation;
            private readonly long _startTimestamp;
            private bool _error;

            private Execution(ThrottlingCircuitBreakerImplementation implementation, long startTimestamp)
            {
                _implementation = implementation;
                _startTimestamp = startTimestamp;
                _error = false;
            }

            public static Execution Begin(ThrottlingCircuitBreakerImplementation implementation)
            {
                var options = implementation._options;
                var aggregatedData = implementation._aggregator.AggregatedInformation;

                var maxThrottle = 1D - Math.Min((double)options.MinCallsPerSamplingInterval / aggregatedData.CallsIncludingThrottled, 1D);
                double minThrottle = 0D;
                if (options.MaxCallsPerSamplingInterval != null)
                    minThrottle = 1D - Math.Min((double)options.MaxCallsPerSamplingInterval / aggregatedData.CallsIncludingThrottled, 1D);

                var throttle = implementation.CurrentThrottle;
                throttle = Math.Max(Math.Min(throttle, maxThrottle), minThrottle);

                if (throttle > 0 && RandomGen.Next() < throttle * int.MaxValue)
                {
                    implementation._aggregator.AddResponse(false, true, 0);
                    throw new BrokenCircuitException($"Calls throttled due to too many exceptions or too long response times. Current throttle factor is: {throttle}.");
                }

                return new Execution(implementation, Stopwatch.GetTimestamp());
            }

            public TResult Result<TResult>(ResultPredicates<TResult> rPred, TResult result)
            {
                if (rPred?.AnyMatch(result) == true)
                {
                    _error = true;
                }

                return result;
            }

            public void Error(ExceptionPredicates exPred, Exception ex)
            {
                if (exPred?.FirstMatchOrDefault(ex) != null)
                {
                    _error = true;
                }
            }

            public void Finish()
            {
                var finishTs = Stopwatch.GetTimestamp();
                _implementation._aggregator.AddResponse(_error, false, GetTimeInMs(_startTimestamp, finishTs));
            }

            private static long GetTimeInMs(long startTs, long finishTs)
            {
                return unchecked((long)((finishTs - startTs) * TickMsFrequency));
            }
        }

        // https://devblogs.microsoft.com/pfxteam/getting-random-numbers-in-a-thread-safe-way/
        private static class RandomGen
        {
            private static readonly Random Global = new Random();

            [ThreadStatic]
            private static Random _local;

            public static int Next()
            {
                Random inst = _local;
                if (inst == null)
                {
                    int seed;
                    lock (Global)
                    {
                        seed = Global.Next();
                    }

                    _local = inst = new Random(seed);
                }

                return inst.Next();
            }
        }
    }
}
