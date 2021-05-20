using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FHICORC.ApplicationHost.Api.Middleware
{
    public class ExceptionInterceptorMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionInterceptorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ILogger<ExceptionInterceptorMiddleware> logger)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException)
            {
                // suppress OperationCanceledExceptions
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected exception: {ExceptionMessage}", ex.Message);
                throw;
            }
        }
    }
}
