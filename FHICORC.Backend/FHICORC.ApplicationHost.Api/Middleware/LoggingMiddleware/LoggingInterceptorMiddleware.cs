using Microsoft.AspNetCore.Http;
using Serilog.Context;
using Serilog.Core;
using FHICORC.Application.Common.Interfaces;
using System;
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

        public async Task Invoke(HttpContext context, ILogEventEnricher logEventEnricher, IMetricLogService metricLogService)
        {
            using (LogContext.Push(logEventEnricher))
            {
                if (context.Request.Path != "/error")
                {
                    metricLogService.AddMetric("API_Path", context.Request.Path);
                    context.Items.Add("StartTime", DateTime.UtcNow);
                }
                await _next(context);
                context.Items.TryGetValue("StartTime", out var startTime);

                if (startTime != null)
                {
                    var timeDiff = DateTime.UtcNow - (DateTime)startTime;
                    metricLogService.AddMetric("API_ElapsedTime", (int)timeDiff.TotalMilliseconds);
                }

                metricLogService.AddMetric("API_StatusCode", context.Response.StatusCode);
                metricLogService.DumpMetricsToLog("API request handled");
            }
        }
    }
}