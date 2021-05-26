using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FHICORC.Application.Models.Options;
using System.Threading.Tasks;

namespace FHICORC.ApplicationHost.Api.Middleware
{
    public class ApiKeyAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiKeyAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILogger<ApiKeyAuthorizationMiddleware> logger, SecurityOptions securityOptions)
        {
            if (securityOptions.CheckApiKeyHeader &&
                context.Features.Get<Microsoft.AspNetCore.Http.Features.IEndpointFeature>().Endpoint?.DisplayName != "Health checks")
            {
                if (!context.Request.Headers.TryGetValue(securityOptions.ApiKeyHeader, out var extractedApiKey))
                {
                    context.Response.StatusCode = 401;
                    logger.LogWarning("Missing ApiKey header");
                    return;
                }

                if (!securityOptions.ApiKey.Equals(extractedApiKey))
                {
                    context.Response.StatusCode = 401;
                    logger.LogWarning("Invalid ApiKey header");
                    return;
                }
            }

            await _next(context);
        }
    }
}

