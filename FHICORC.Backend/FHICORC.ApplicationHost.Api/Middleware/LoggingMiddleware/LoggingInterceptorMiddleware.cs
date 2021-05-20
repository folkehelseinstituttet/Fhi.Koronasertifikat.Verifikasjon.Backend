using Microsoft.AspNetCore.Http;
using Serilog.Context;
using Serilog.Core;
using System.Threading.Tasks;

namespace FHICORC.ApplicationHost.Api.Middleware.LoggingMiddleware
{
    public class LoggingInterceptorMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingInterceptorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ILogEventEnricher logEventEnricher)
        {
            using (LogContext.Push(logEventEnricher))
            {
                await _next(context);
            }
        }
    }
}