using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog.Context;
using Serilog.Core;
using System.Threading.Tasks;
using FHICORC.Application.Common.Logging.Metrics;
using Microsoft.Extensions.Logging;

namespace FHICORC.ApplicationHost.Api.Middleware.LoggingMiddleware
{
    public class LoggingInterceptorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Stopwatch _stopwatch;

        public LoggingInterceptorMiddleware(RequestDelegate next)
        {
            _next = next;
            _stopwatch = new Stopwatch();
        }

        public async Task Invoke(HttpContext context, ILogEventEnricher logEventEnricher, ILogger<LoggingInterceptorMiddleware> logger)
        {
            using (LogContext.Push(logEventEnricher))
            {
                _stopwatch.Start();
                await _next(context);
                _stopwatch.Stop();
                logger.MetricLogMessage("API call with path '{APIPath}' finished in {ElapsedTime} ms. API response status code: {APIStatusCode}.",
                    context.Request.Path, _stopwatch.ElapsedMilliseconds, context.Response.StatusCode);
                _stopwatch.Reset();
            }
        }
    }
}