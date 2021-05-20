using Serilog.Core;
using Serilog.Events;

namespace FHICORC.ApplicationHost.Api.Middleware.LoggingMiddleware
{
    public class CommonEventEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            // Add common log properties, like servicename etc.
        }
    }
}
