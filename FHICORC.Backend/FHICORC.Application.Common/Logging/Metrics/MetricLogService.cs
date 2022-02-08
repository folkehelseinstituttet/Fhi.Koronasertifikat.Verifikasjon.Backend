using Microsoft.Extensions.Logging;
using FHICORC.Application.Common.Interfaces;
using System.Collections.Generic;

namespace FHICORC.Application.Common.Logging.Metrics
{
    public class MetricLogService : IMetricLogService
    {
        private readonly ILogger<MetricLogService> _logger;
        private readonly Dictionary<string, string> _metricDictionary;

        public MetricLogService(ILogger<MetricLogService> logger)
        {
            _logger = logger;
            _metricDictionary = new Dictionary<string, string>();
        }

        public void AddMetric(string metricName, string metricValue)
        {            
            if (!_metricDictionary.TryAdd(metricName, metricValue))
            {
                _logger.LogWarning($"Metric already added {metricName}", metricName);
            }
        }

        public void AddMetric(string metricName, object metricValue)
        {
            AddMetric(metricName, metricValue.ToString());
        }

        public void DumpMetricsToLog(string logMessageTemplate)
        {
            if (_metricDictionary.Count > 0)
            {
                _logger.LogInformation(logMessageTemplate + ", with {Metrics}", _metricDictionary);
            }
        }
    }
}
