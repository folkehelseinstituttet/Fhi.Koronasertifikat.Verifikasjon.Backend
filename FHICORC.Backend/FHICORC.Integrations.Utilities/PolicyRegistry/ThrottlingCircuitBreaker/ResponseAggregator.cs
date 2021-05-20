using System;
using System.Collections.Generic;

namespace FHICORC.Integrations.Utilities.PolicyRegistry.ThrottlingCircuitBreaker
{
    public class ResponseAggregator
    {
        private readonly object _aggregateLock = new();
        private readonly Queue<ResponseInfo> _aggregate = new();
        private long _aggregateErrors = 0;
        private long _aggregateCalls = 0;
        private long _aggregateExecutionTimeMs = 0;

        public int AggregationTimeMs { get; }

        public ResponseAggregator(int aggregationTime)
        {
            AggregationTimeMs = aggregationTime;
        }

        public AggregatedResponseInformation AggregatedInformation
        {
            get
            {
                lock (_aggregateLock)
                {
                    PruneAggregates_RequiresLock();
                    if (_aggregateCalls == 0)
                        return new AggregatedResponseInformation { CallsIncludingThrottled = _aggregate.Count };

                    return new AggregatedResponseInformation
                    {
                        ErrorPercentage = 100D * _aggregateErrors / _aggregateCalls,
                        AverageExecutionTimeMs = _aggregateExecutionTimeMs / _aggregateCalls,
                        Calls = _aggregateCalls,
                        CallsIncludingThrottled = _aggregate.Count
                    };
                }
            }
        }

        public void AddResponse(bool error, bool throttled, long executionTimeMs)
        {
            lock (_aggregateLock)
            {
                _aggregate.Enqueue(new ResponseInfo
                {
                    Error = error,
                    Throttled = throttled,
                    ExecutionTimeMs = executionTimeMs,
                    FinishedTime = DateTime.Now
                });

                if (!throttled)
                {
                    ++_aggregateCalls;

                    if (error)
                    {
                        ++_aggregateErrors;
                    }

                    _aggregateExecutionTimeMs += executionTimeMs;
                }
            }
        }

        private void PruneAggregates_RequiresLock()
        {
            DateTime pruneBefore = DateTime.Now.AddMilliseconds(-AggregationTimeMs);

            if (_aggregate.Count == 0)
            {
                return;
            }

            var info = _aggregate.Peek();
            while (info.FinishedTime < pruneBefore)
            {
                _aggregate.Dequeue();

                if (info.Error)
                {
                    --_aggregateErrors;
                }

                if (!info.Throttled)
                {
                    --_aggregateCalls;
                }

                _aggregateExecutionTimeMs -= info.ExecutionTimeMs;

                if (_aggregate.Count == 0)
                {
                    return;
                }

                info = _aggregate.Peek();
            }
        }

        private struct ResponseInfo
        {
            public bool Error { get; init; }
            public bool Throttled { get; set; }
            public long ExecutionTimeMs { get; init; }
            public DateTime FinishedTime { get; init; }
        }
    }
}
