
namespace FHICORC.Application.Common.Interfaces
{
    public interface IMetricLogService
    {
        void AddMetric(string metricName, string metricValue);
        void AddMetric(string metricName, object metricValue);
        void DumpMetricsToLog(string logMessageTemplate);
    }
}
