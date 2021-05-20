﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog.Core;
using FHICORC.ApplicationHost.Api.Middleware.LoggingMiddleware;
using FHICORC.ApplicationHost.Api.Middleware.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FHICORC.ApplicationHost.Api.Middleware
{
    public static class ApplicationBuilderExtensions
    {
        public static IServiceCollection AddServiceDependencies(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddScoped<ILogEventEnricher, CommonEventEnricher>();
        }

        public static IApplicationBuilder UseServiceMiddleware(this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder
                .UseMiddleware<LoggingInterceptorMiddleware>()
                .UseMiddleware<ExceptionInterceptorMiddleware>()
                .UseMiddleware<ApiKeyAuthorizationMiddleware>();
        }

        public static IServiceCollection AddConfiguredSwaggerGen(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSwaggerGen();
            serviceCollection.AddSingleton<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGenOptions>();
            return serviceCollection;
        }
    }
}