using Microsoft.Extensions.Logging;

namespace FHICORC.Application.Common.Logging.Metrics
{
    public static class MetricLogger
    {
        private const string METRIC_KEY = "<Metric>";
        public static void MetricLogElapsedTime(this ILogger logger, string message, long elapsedTime)
        {
            logger.LogInformation(METRIC_KEY + message, elapsedTime);
        }

        public static void MetricLogMessage(this ILogger logger, string message)
        {
            logger.LogInformation(METRIC_KEY + message);
        }

        public static void MetricLogMessage(this ILogger logger, string message, params object[] args)
        {
            logger.LogInformation(METRIC_KEY + message, args);
        }
    }
}
